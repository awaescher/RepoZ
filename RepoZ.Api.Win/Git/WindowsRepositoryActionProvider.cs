using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.IO;
using GongSolutions.Shell;
using System.Drawing;
using RepoZ.Api.Git;
using RepoZ.Api.Common;

namespace RepoZ.Api.Win.IO
{
	public class WindowsRepositoryActionProvider : IRepositoryActionProvider
	{
		private readonly IRepositoryWriter _repositoryWriter;
		private readonly IErrorHandler _errorHandler;

		public WindowsRepositoryActionProvider(IRepositoryWriter repositoryWriter, IErrorHandler errorHandler)
		{
			_repositoryWriter = repositoryWriter;
			_errorHandler = errorHandler;
		}

		public RepositoryAction GetPrimaryAction(Repository repository)
		{
			return CreateProcessRunnerAction("Open in Windows File Explorer", repository.Path);
		}

		public RepositoryAction GetSecondaryAction(Repository repository)
		{
			return CreateProcessRunnerAction("Open in Windows PowerShell", "powershell.exe ", $"-noexit -command \"cd '{repository.Path}'\"");
		}

		public IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories)
		{
			var singleRepository = repositories.Count() == 1 ? repositories.Single() : null;

			if (singleRepository != null)
			{
				yield return GetPrimaryAction(singleRepository);
				yield return GetSecondaryAction(singleRepository);
				yield return CreateProcessRunnerAction("Open in Windows Command Prompt (cmd.exe)", "cmd.exe", $"/K \"cd /d {singleRepository.Path}\"");

				string bashSubpath = @"Git\git-bash.exe";
				string folder = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
				string gitbash = Path.Combine(folder, bashSubpath);

				if (!File.Exists(gitbash))
				{
					folder = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
					gitbash = Path.Combine(folder, bashSubpath);
				}

				if (File.Exists(gitbash))
				{
					string path = singleRepository.Path;
					if (path.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
						path = path.Substring(0, path.Length - 1);
					yield return CreateProcessRunnerAction("Open in Git Bash", gitbash, $"\"--cd={path}\"");
				}
			}
			yield return CreateActionForMultipleRepositories("Fetch", repositories, _repositoryWriter.Fetch, beginGroup:true, executionCausesSynchronizing: true);
			yield return CreateActionForMultipleRepositories("Pull", repositories, _repositoryWriter.Pull, executionCausesSynchronizing:true);
			yield return CreateActionForMultipleRepositories("Push", repositories, _repositoryWriter.Push, executionCausesSynchronizing: true);

			if (singleRepository != null)
			{
				yield return new RepositoryAction()
				{
					Name = "Checkout",
					SubActions = singleRepository.LocalBranches.Select(branch => new RepositoryAction()
					{
						Name = branch,
						Action = (s, e) => _repositoryWriter.Checkout(singleRepository, branch),
						CanExecute = !singleRepository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase)
					}).ToArray()
				};

				yield return new RepositoryAction()
				{
					Name = "Shell",
					Action = (sender, args) =>
					{
						var coords = args as float[];

						var i = new ShellItem(singleRepository.Path);
						var m = new ShellContextMenu(i);
						m.ShowContextMenu(new System.Windows.Forms.Button(), new Point((int)coords[0], (int)coords[1]));
					},
					BeginGroup = true
				};
			}
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
				Process.Start(process, arguments);
			}
			catch (Exception ex)
			{
				_errorHandler.Handle(ex.Message);
			}
		}
	}
}
