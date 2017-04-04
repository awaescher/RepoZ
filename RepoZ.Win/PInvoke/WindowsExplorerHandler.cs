using RepoZ.Api.Git;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Win.PInvoke
{
	public class WindowsExplorerHandler
	{
		private IRepositoryReader _repositoryReader;
		private Type _shellApplicationType;

		public WindowsExplorerHandler(IRepositoryReader repositoryReader)
		{
			_repositoryReader = repositoryReader;
		}

		public bool CanHandle(string processName)
		{
			return string.Equals("explorer", processName, StringComparison.OrdinalIgnoreCase);
		}

		public string Pulse()
		{
			if (_shellApplicationType == null)
				_shellApplicationType = Type.GetTypeFromProgID("Shell.Application");

			dynamic o = Activator.CreateInstance(_shellApplicationType);
			try
			{
				var ws = o.Windows();
				for (int i = 0; i < ws.Count; i++)
				{
					var ie = ws.Item(i);
					if (ie == null)
						continue;

					var executable = System.IO.Path.GetFileName((string)ie.FullName);
					if (executable.ToLower() == "explorer.exe")
					{
						string path = ie?.document?.focuseditem?.path ?? "n/a"; // ist das fokussierte, nicht das aktuelle
						WindowHelper.AppendWindowText((IntPtr)ie.hwnd, " # ", path);
					}
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(o);
			}

			return null;
		}
	}
}
