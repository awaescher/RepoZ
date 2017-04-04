using RepoZ.Api.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Shared.PathFinding
{
	public class WindowsExplorerPathFinder : IPathFinder
	{
		private Type _shellApplicationType;

		public bool CanHandle(string processName)
		{
			return string.Equals("explorer", processName, StringComparison.OrdinalIgnoreCase);
		}

		public string FindPath(IntPtr windowHandle)
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
					if (ie == null || ie.hwnd != (long)windowHandle)
						continue;

					var path = System.IO.Path.GetFileName((string)ie.FullName);
					if (path.ToLower() == "explorer.exe")
						return ie?.document?.focuseditem?.path;
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
