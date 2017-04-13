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

namespace RepoZ.Api.Win.IO
{
	public class WindowsRepositoryActionProvider : IRepositoryActionProvider
	{
		public IEnumerable<RepositoryAction> GetFor(Repository repository)
		{
			yield return createDefaultAction("Open in Windows File Explorer", repository.Path);
			yield return createAction("Open in Windows Command Prompt (cmd.exe)", "cmd.exe", $"/K \"cd /d {repository.Path}\"");
			yield return createAction("Open in Windows PowerShell", "powershell.exe ", $"-noexit -command \"cd '{repository.Path}'\"");

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
				string path = repository.Path;
				if (path.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
					path = path.Substring(0, path.Length - 1);
				yield return createAction("Open in Git Bash", gitbash, $"\"--cd={path}\"");
			}

			yield return new RepositoryAction()
			{
				Name = "Shell",
				Action = (sender, args) =>
				{
					var coords = args as float[];

					var i = new ShellItem(repository.Path);
					var m = new ShellContextMenu(i);
					m.ShowContextMenu(new System.Windows.Forms.Button(), new Point((int)coords[0], (int)coords[1]));
				},
				BeginGroup = true
			};
		}

		private RepositoryAction createAction(string name, string process, string arguments = "")
		{
			return new RepositoryAction()
			{
				Name = name,
				Action = (sender, args) => startProcess(process, arguments)
			};
		}

		private RepositoryAction createDefaultAction(string name, string process, string arguments = "")
		{
			var action = createAction(name, process, arguments);
			action.IsDefault = true;
			return action;
		}

		private void startProcess(string process, string arguments)
		{
			Process.Start(process, arguments);
		}
	}
}
