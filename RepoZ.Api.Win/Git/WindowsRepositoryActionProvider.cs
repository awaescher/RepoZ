﻿using RepoZ.Api.Common;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Git;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RepoZ.Api.Win.IO
{
	public class WindowsRepositoryActionProvider : IRepositoryActionProvider
	{
		private readonly IRepositoryWriter _repositoryWriter;
		private readonly IRepositoryMonitor _repositoryMonitor;
		private readonly IErrorHandler _errorHandler;
		private readonly ITranslationService _translationService;

		private string _windowsTerminalLocation;
		private string _codeLocation;
		private string _sourceTreeLocation;

		public WindowsRepositoryActionProvider(
			IRepositoryWriter repositoryWriter,
			IRepositoryMonitor repositoryMonitor,
			IErrorHandler errorHandler,
			ITranslationService translationService)
		{
			_repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
			_repositoryMonitor = repositoryMonitor ?? throw new ArgumentNullException(nameof(repositoryMonitor));
			_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
			_translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
		}

		public RepositoryAction GetPrimaryAction(Repository repository)
		{
			return CreateProcessRunnerAction(_translationService.Translate("Open in Windows File Explorer"), repository.SafePath);
		}

		public RepositoryAction GetSecondaryAction(Repository repository)
		{
			if (HasWindowsTerminal())
				return CreateProcessRunnerAction(_translationService.Translate("Open in Windows Terminal"), "wt.exe ", $"-d \"{repository.SafePath}\"");

			return CreateProcessRunnerAction(_translationService.Translate("Open in Windows PowerShell"), "powershell.exe ", $"-executionpolicy bypass -noexit -command \"Set-Location '{repository.SafePath}'\"");
		}

		public IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories)
		{
			return GetContextMenuActionsInternal(repositories.Where(r => Directory.Exists(r.SafePath))).Where(a => a != null);
		}

		private IEnumerable<RepositoryAction> GetContextMenuActionsInternal(IEnumerable<Repository> repositories)
		{
			var singleRepository = repositories.Count() == 1 ? repositories.Single() : null;

			if (singleRepository != null)
			{
				yield return GetPrimaryAction(singleRepository);
				yield return GetSecondaryAction(singleRepository);

				var codeExecutable = TryFindCode();
				var hasCode = !string.IsNullOrEmpty(codeExecutable);
				if (hasCode)
					yield return CreateProcessRunnerAction(_translationService.Translate("Open in Visual Studio Code"), codeExecutable, '"' + singleRepository.SafePath + '"');

				yield return CreateFileActionSubMenu(singleRepository, _translationService.Translate("Open Visual Studio solutions"), "*.sln");

				var sourceTreeExecutable = TryFindSourceTree();
				var hasSourceTree = !string.IsNullOrEmpty(sourceTreeExecutable);
				if (hasSourceTree)
					yield return CreateProcessRunnerAction(_translationService.Translate("Open in Sourcetree"), sourceTreeExecutable, "-f " + '"' + singleRepository.SafePath + '"');


				yield return CreateBrowseRemoteAction(singleRepository);
			}

			yield return CreateActionForMultipleRepositories(_translationService.Translate("Fetch"), repositories, _repositoryWriter.Fetch, beginGroup: true, executionCausesSynchronizing: true);
			yield return CreateActionForMultipleRepositories(_translationService.Translate("Pull"), repositories, _repositoryWriter.Pull, executionCausesSynchronizing: true);
			yield return CreateActionForMultipleRepositories(_translationService.Translate("Push"), repositories, _repositoryWriter.Push, executionCausesSynchronizing: true);

            if (singleRepository != null)
            {
				// Strip label of "(r)" and "(l)" indicators
                yield return new RepositoryAction()
                {
                    Name = _translationService.Translate("Checkout"),
                    DeferredSubActionsEnumerator = () => singleRepository.ReadAllBranches()
                                                             .Take(50)
                                                             .Select(branch => new RepositoryAction()
                                                             {
                                                                 Name = branch,
                                                                 Action = (_, __) => _repositoryWriter.Checkout(singleRepository, branch.Replace(" (r)", "").Replace(" (l)", "")),
                                                                 CanExecute = !singleRepository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase)
                                                             })
                                                             .ToArray()
                };
            }

			yield return CreateActionForMultipleRepositories(_translationService.Translate("Ignore"), repositories, r => _repositoryMonitor.IgnoreByPath(r.Path), beginGroup: true);
		}

		private RepositoryAction CreateProcessRunnerAction(string name, string process, string arguments = "", bool beginGroup = false)
		{
			return new RepositoryAction()
			{
				BeginGroup = beginGroup,
				Name = name,
				Action = (_, __) => StartProcess(process, arguments)
			};
		}

		private RepositoryAction CreateActionForMultipleRepositories(string name,
			IEnumerable<Repository> repositories,
			Action<Repository> action,
			bool beginGroup = false,
			bool executionCausesSynchronizing = false)
		{
			return new RepositoryAction()
			{
				Name = name,
				Action = (_, __) =>
				{
					// copy over to an array to not get an exception
					// once the enumerator changes (which can happen when a change
					// is detected and a repository is renewed) while the loop is running
					var repositoryArray = repositories.ToArray();

					foreach (var repository in repositoryArray)
						SafelyExecute(action, repository); // git/io-exceptions will break the loop, put in try/catch
				},
				BeginGroup = beginGroup,
				ExecutionCausesSynchronizing = executionCausesSynchronizing
			};
		}

		private void SafelyExecute(Action<Repository> action, Repository repository)
		{
			try
			{
				action(repository);
			}
			catch
			{
				// nothing to see here
			}
		}

		private void StartProcess(string process, string arguments)
		{
			try
			{
				Debug.WriteLine("Starting: " + process + arguments);
				Process.Start(process, arguments);
			}
			catch (Exception ex)
			{
				_errorHandler.Handle(ex.Message);
			}
		}

		private bool HasWindowsTerminal() => !string.IsNullOrEmpty(TryFindWindowsTerminal());

		private string TryFindWindowsTerminal()
		{
			if (_windowsTerminalLocation == null)
			{
				var executable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "wt.exe");
				_windowsTerminalLocation = File.Exists(executable) ? executable : "";
			}

			return _windowsTerminalLocation;
		}

		private string TryFindSourceTree()
		{
			if (_sourceTreeLocation == null)
			{
				//Try : Installed in the user profile
				var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var executable = Path.Combine(folder, "SourceTree", "SourceTree.exe");

				_sourceTreeLocation = File.Exists(executable) ? executable : string.Empty;

				//Try: Installed in Program files x86
				if (string.IsNullOrEmpty(_sourceTreeLocation))
				{
					folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
					folder = Path.Combine(folder, "Atlassian");
					executable = Path.Combine(folder, "SourceTree", "SourceTree.exe");

					_sourceTreeLocation = File.Exists(executable) ? executable : string.Empty;
				}
			}
			return _sourceTreeLocation;
		}

		private string TryFindCode()
		{
			if (_codeLocation == null)
			{
				var sub = Path.Combine("Microsoft VS Code", "code.exe");
				var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var executable = Path.Combine(folder, "Programs", sub);

				_codeLocation = File.Exists(executable) ? executable : "";

				if (string.IsNullOrEmpty(_codeLocation))
				{
					folder = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
					executable = Path.Combine(folder, sub);

					_codeLocation = File.Exists(executable) ? executable : "";
				}
			}

			return _codeLocation;
		}

		private RepositoryAction CreateFileActionSubMenu(Repository repository, string actionName, string filePattern)
		{
			if (HasFiles(repository, filePattern))
			{
				return new RepositoryAction()
				{
					Name = actionName,
					DeferredSubActionsEnumerator = () =>
								GetFiles(repository, filePattern)
								.Select(sln => CreateProcessRunnerAction(Path.GetFileName(sln), sln))
								.ToArray()
				};
			}

			return null;
		}

		private RepositoryAction CreateBrowseRemoteAction(Repository repository)
		{
			if (repository.RemoteUrls.Length == 0)
				return null;

			var actionName = _translationService.Translate("Browse remote");

			if (repository.RemoteUrls.Length == 1)
			{
				return CreateProcessRunnerAction(actionName, repository.RemoteUrls[0]);
			}

			return new RepositoryAction()
			{
				Name = actionName,
				DeferredSubActionsEnumerator = () => repository.RemoteUrls
														 .Take(50)
														 .Select(url => CreateProcessRunnerAction(url, url))
														 .ToArray()
			};
		}

		private bool HasFiles(Repository repository, string searchPattern)
		{
			return GetFileEnumerator(repository, searchPattern).Any();
		}

		private IEnumerable<string> GetFiles(Repository repository, string searchPattern)
		{
			return GetFileEnumerator(repository, searchPattern)
				.Take(25)
				.OrderBy(f => f.Name)
				.Select(f => f.FullName);
		}

		private IEnumerable<FileInfo> GetFileEnumerator(Repository repository, string searchPattern)
		{
			var directory = new DirectoryInfo(repository.Path);
			return directory
				.EnumerateFiles(searchPattern, SearchOption.AllDirectories)
				.Where(f => !f.Name.StartsWith("."));
		}
	}
}
