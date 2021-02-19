using RepoZ.Api.Common;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.Git;
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
		private readonly IRepositoryActionConfigurationStore _repositoryActionConfigurationStore;
		private readonly IRepositoryWriter _repositoryWriter;
		private readonly IRepositoryMonitor _repositoryMonitor;
		private readonly IErrorHandler _errorHandler;
		private readonly ITranslationService _translationService;

		public WindowsRepositoryActionProvider(
			IRepositoryActionConfigurationStore repositoryActionConfigurationStore,
			IRepositoryWriter repositoryWriter,
			IRepositoryMonitor repositoryMonitor,
			IErrorHandler errorHandler,
			ITranslationService translationService)
		{
			_repositoryActionConfigurationStore = repositoryActionConfigurationStore ?? throw new ArgumentNullException(nameof(repositoryActionConfigurationStore));
			_repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
			_repositoryMonitor = repositoryMonitor ?? throw new ArgumentNullException(nameof(repositoryMonitor));
			_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
			_translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
		}

		public RepositoryAction GetPrimaryAction(Repository repository)
		{
			var actions = GetContextMenuActions(new[] { repository });
			return actions.FirstOrDefault();
		}

		public RepositoryAction GetSecondaryAction(Repository repository)
		{
			var actions = GetContextMenuActions(new[] { repository });
			return actions.Count() > 1 ? actions.ElementAt(1) : null;
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
				var configuration = _repositoryActionConfigurationStore.RepositoryActionConfiguration;

				foreach (var action in configuration.RepositoryActions.Where(a => a.Active))
				{
					yield return CreateProcessRunnerAction(
						_translationService.Translate(action.Name),
						action.Executables.Select(e => ReplaceVariables(e, singleRepository)),
						ReplaceVariables(action.Arguments, singleRepository),
						beginGroup: false);
				}

				foreach (var fileAssociaction in configuration.FileAssociations.Where(a => a.Active))
				{
					yield return CreateFileActionSubMenu(
						singleRepository,
						_translationService.Translate(fileAssociaction.Name),
						fileAssociaction.Extension);
				}

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
					DeferredSubActionsEnumerator = () => singleRepository.AllBranches
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

		private string ReplaceVariables(string value, Repository repository)
		{
			if (value is null)
				return string.Empty;

			return Environment.ExpandEnvironmentVariables(
				value
				.Replace("{Repository.Name}", repository.Name)
				.Replace("{Repository.Path}", repository.Path)
				.Replace("{Repository.SafePath}", repository.SafePath)
				.Replace("{Repository.Location}", repository.Location)
				.Replace("{Repository.CurrentBranch}", repository.CurrentBranch)
				.Replace("{Repository.Branches}", string.Join("|", repository.Branches))
				.Replace("{Repository.LocalBranches}", string.Join("|", repository.LocalBranches))
				.Replace("{Repository.RemoteUrls}", string.Join("|", repository.RemoteUrls)));
		}

		private RepositoryAction CreateProcessRunnerAction(string name, IEnumerable<string> executables, string arguments = "", bool beginGroup = false)
		{
			foreach (var executable in executables)
			{
				var normalized = executable.Replace("\"", "");
				if (File.Exists(normalized) || Directory.Exists(normalized))
				{
					return new RepositoryAction()
					{
						BeginGroup = beginGroup,
						Name = name,
						Action = (_, __) => StartProcess(executable, arguments)
					};
				}
			}

			return null;
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

		private RepositoryAction CreateFileActionSubMenu(Repository repository, string actionName, string filePattern)
		{
			if (HasFiles(repository, filePattern))
			{
				return new RepositoryAction()
				{
					Name = actionName,
					DeferredSubActionsEnumerator = () =>
								GetFiles(repository, filePattern)
								.Select(solutionFile => ReplaceVariables(solutionFile, repository))
								.Select(solutionFile => CreateProcessRunnerAction(Path.GetFileName(solutionFile), new[] { solutionFile }))
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
				return CreateProcessRunnerAction(actionName, new[] { repository.RemoteUrls[0] });
			}

			return new RepositoryAction()
			{
				Name = actionName,
				DeferredSubActionsEnumerator = () => repository.RemoteUrls
														 .Take(50)
														 .Select(url => CreateProcessRunnerAction(url, new[] { url }))
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
