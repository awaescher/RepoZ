using branch.Shared.PathFinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace branch.Shared
{
	public class WindowFinder
	{
		private IEnumerable<IPathFinder> _pathFinders;

		public WindowFinder(IEnumerable<IPathFinder> pathFinders)
		{
			_pathFinders = pathFinders;
		}

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		private IntPtr FindActiveWindow()
		{
			return GetForegroundWindow();
		}

		public string GetPathOfCurrentWindow()
		{
			IntPtr handle = FindActiveWindow();

			uint procId;
			GetWindowThreadProcessId(handle, out procId);

			var proc = Process.GetProcessById((int)procId);
			string processName = proc?.ProcessName;

			if (string.IsNullOrEmpty(processName))
				return string.Empty;

			var finder = _pathFinders.FirstOrDefault(f => f.CanHandle(processName));

			return finder?.FindPath(handle) ?? string.Empty;
		}
	}
}
