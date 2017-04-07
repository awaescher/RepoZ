using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.IO;
using GongSolutions.Shell;

namespace RepoZ.Api.Win.IO
{
	public class WindowsPathActionProvider : IPathActionProvider
	{
		public IEnumerable<PathAction> GetFor(string path)
		{
			yield return createPathAction("Open in Windows File Explorer", path);
			yield return createPathAction("Open in Windows Command Prompt (cmd.exe)", "cmd.exe", $"/K \"cd /d {path}\"");
			yield return createPathAction("Open in Windows PowerShell", "powershell.exe ", $"-noexit -command \"cd '{path}'\"");

			// TODO
			string folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			folder = @"C:\Program Files";
			string exec = Path.Combine(folder, @"Git\bin\sh.exe");
			if (File.Exists(exec))
				yield return createPathAction("Open in Git Bash", "cmd.exe", $"/c (start /b /i *C:\\* *{exec}*) && exit".Replace("*", "\""));
			else
			{
				folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
				exec = Path.Combine(folder, @"Git\bin\sh.exe");
				if (File.Exists(exec))
					yield return createPathAction("Open in Git Bash", "cmd.exe", $"/c (start /b /i *%cd%* *{exec}*) && exit".Replace("*", "\""));
			}

            yield return new PathAction()
            {
                Name = "Shell",
                Action = () =>
                {
                    var i = new ShellItem(path);
                    var m = new ShellContextMenu(i);
                    m.ShowContextMenu(new System.Windows.Forms.Button(), System.Windows.Forms.Control.MousePosition);
                },
                BeginGroup = true
            };
    }

    private PathAction createPathAction(string name, string process, string arguments = "")
		{
			return new PathAction()
			{
				Name = name,
				Action = startProcess(process, arguments)
			};
		}

		private Action startProcess(string process, string arguments)
		{
			return () => Process.Start(process, arguments);
		}
	}
}
