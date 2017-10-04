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
		private IRepositoryWriter _repositoryWriter;
		private IErrorHandler _errorHandler;

		public WindowsRepositoryActionProvider(IRepositoryWriter repositoryWriter, IErrorHandler errorHandler)
		{
			_repositoryWriter = repositoryWriter;
			_errorHandler = errorHandler;
		}

		public IEnumerable<RepositoryAction> GetFor(IEnumerable<Repository> repositories)
		{
			var singleRepository = repositories.Count() == 1 ? repositories.Single() : null;

			if (singleRepository != null)
			{
				var defaultAction = CreateProcessRunnerAction("Open in Windows File Explorer", singleRepository.Path);
				defaultAction.IsDefault = true;
				yield return defaultAction;
				yield return CreateProcessRunnerAction("Open in Windows Command Prompt (cmd.exe)", "cmd.exe", $"/K \"cd /d {singleRepository.Path}\"");
				yield return CreateProcessRunnerAction("Open in Windows PowerShell", "powershell.exe ", $"-noexit -command \"cd '{singleRepository.Path}'\"");

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
			yield return CreateActionForMultipleRepositories("Fetch", repositories, _repositoryWriter.Fetch, beginGroup:true);
			yield return CreateActionForMultipleRepositories("Pull", repositories, _repositoryWriter.Pull);
			yield return CreateActionForMultipleRepositories("Push", repositories, _repositoryWriter.Push);

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

		private RepositoryAction CreateActionForMultipleRepositories(string name, IEnumerable<Repository> repositories, Action<Repository> action, bool beginGroup = false)
		{
			return new RepositoryAction()
			{
				Name = name,
				Action = (sender, args) =>
				{
					foreach (var repository in repositories)
						action(repository);
				},
				BeginGroup = beginGroup
			};
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
