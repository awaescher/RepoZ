using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.IO;
using System.Drawing;
using RepoZ.Api.Git;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Common;

namespace RepoZ.Api.Win.IO
{
	public class WindowsRepositoryActionProvider : IRepositoryActionProvider
	{
		private readonly IRepositoryWriter _repositoryWriter;
		private readonly IRepositoryMonitor _repositoryMonitor;
		private readonly IErrorHandler _errorHandler;
		private readonly ITranslationService _translationService;

		private string _windowsTerminalLocation;
		private string _bashLocation;
		private string _codeLocation;

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
			var singleRepository = repositories.Count() == 1 ? repositories.Single() : null;

			if (singleRepository != null)
			{
				yield return GetPrimaryAction(singleRepository);
				yield return GetSecondaryAction(singleRepository);

				// if Windows Terminal is installed, provider PowerShell as additional option here (otherwise PowerShell is the secondary action)
				if (HasWindowsTerminal())
					yield return CreateProcessRunnerAction(_translationService.Translate("Open in Windows PowerShell"), "powershell.exe ", $"-executionpolicy bypass -noexit -command \"Set-Location '{singleRepository.SafePath}'\"");

				yield return CreateProcessRunnerAction(_translationService.Translate("Open in Windows Command Prompt"), "cmd.exe", $"/K \"cd /d {singleRepository.SafePath}\"");

				var bashExecutable = TryFindBash();
				var hasBash = !string.IsNullOrEmpty(bashExecutable);
				if (hasBash)
				{
					string path = singleRepository.SafePath;
					if (path.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
						path = path.Substring(0, path.Length - 1);
					yield return CreateProcessRunnerAction(_translationService.Translate("Open in Git Bash"), bashExecutable, $"\"--cd={path}\"");
				}

				var codeExecutable = TryFindCode();
				var hasCode = !string.IsNullOrEmpty(codeExecutable);
				if (hasCode)
					yield return CreateProcessRunnerAction(_translationService.Translate("Open in Visual Studio Code"), codeExecutable, singleRepository.SafePath);

				var slnFiles = TryFindVisualStudioSlnFiles(singleRepository);
				foreach (var slnFile in slnFiles)
				{
					yield return CreateProcessRunnerAction(_translationService.Translate("Open {0}", slnFile), Path.Combine(singleRepository.Path, slnFile));
				}

			}

			yield return CreateActionForMultipleRepositories(_translationService.Translate("Fetch"), repositories, _repositoryWriter.Fetch, beginGroup: true, executionCausesSynchronizing: true);
			yield return CreateActionForMultipleRepositories(_translationService.Translate("Pull"), repositories, _repositoryWriter.Pull, executionCausesSynchronizing: true);
			yield return CreateActionForMultipleRepositories(_translationService.Translate("Push"), repositories, _repositoryWriter.Push, executionCausesSynchronizing: true);

			if (singleRepository != null)
			{
				yield return new RepositoryAction()
				{
					Name = _translationService.Translate("Checkout"),
					SubActions = singleRepository.LocalBranches.Select(branch => new RepositoryAction()
					{
						Name = branch,
						Action = (s, e) => _repositoryWriter.Checkout(singleRepository, branch),
						CanExecute = !singleRepository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase)
					}).ToArray()
				};
			}

			yield return CreateActionForMultipleRepositories(_translationService.Translate("Ignore"), repositories, r => _repositoryMonitor.IgnoreByPath(r.Path), beginGroup: true);
		}

		private RepositoryAction CreateProcessRunnerAction(string name, string process, string arguments = "")
		{
			return new RepositoryAction()
			{
				Name = name,
				Action = (sender, args) => StartProcess(process, arguments)
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
				Action = (sender, args) =>
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

		private string TryFindBash()
		{
			if (_bashLocation == null)
			{
				var sub = Path.Combine("Git", "git-bash.exe");
				var folder = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
				var executable = Path.Combine(folder, sub);

				_bashLocation = File.Exists(executable) ? executable : "";

				if (string.IsNullOrEmpty(_bashLocation))
				{
					folder = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
					executable = Path.Combine(folder, sub);

					_bashLocation = File.Exists(executable) ? executable : "";
				}
			}

			return _bashLocation;
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

		private IEnumerable<string> TryFindVisualStudioSlnFiles(Repository repository)
		{
			var directoryInfo = new DirectoryInfo(repository.Path);
			return directoryInfo.GetFiles("*.sln").Select(f => f.Name);
		}
    }
}
