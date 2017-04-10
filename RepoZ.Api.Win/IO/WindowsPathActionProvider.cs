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

namespace RepoZ.Api.Win.IO
{
	public class WindowsPathActionProvider : IPathActionProvider
	{
		public IEnumerable<PathAction> GetFor(string path)
		{
			yield return createDefaultPathAction("Open in Windows File Explorer", path);
			yield return createPathAction("Open in Windows Command Prompt (cmd.exe)", "cmd.exe", $"/K \"cd /d {path}\"");
			yield return createPathAction("Open in Windows PowerShell", "powershell.exe ", $"-noexit -command \"cd '{path}'\"");

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
				if (path.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
					path = path.Substring(0, path.Length - 1);
				yield return createPathAction("Open in Git Bash", gitbash, $"\"--cd={path}\"");
			}

			yield return new PathAction()
			{
				Name = "Shell",
				Action = (sender, args) =>
				{
					var coords = args as float[];

					var i = new ShellItem(path);
					var m = new ShellContextMenu(i);
					m.ShowContextMenu(new System.Windows.Forms.Button(), new Point((int)coords[0], (int)coords[1]));
				},
				BeginGroup = true
			};
		}

		private PathAction createPathAction(string name, string process, string arguments = "")
		{
			return new PathAction()
			{
				Name = name,
				Action = (sender, args) => startProcess(process, arguments)
			};
		}

		private PathAction createDefaultPathAction(string name, string process, string arguments = "")
		{
			var action = createPathAction(name, process, arguments);
			action.IsDefault = true;
			return action;
		}

		private Action startProcess(string process, string arguments)
		{
			return () => Process.Start(process, arguments);
		}
	}
}
