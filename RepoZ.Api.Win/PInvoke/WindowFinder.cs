using RepoZ.Api.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.PInvoke
{
	public class WindowFinder
	{
		private readonly IEnumerable<IPathFinder> _pathFinders;

		public WindowFinder(IEnumerable<IPathFinder> pathFinders)
		{
			_pathFinders = pathFinders;
		}

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
		public static extern bool SetWindowText(IntPtr hWnd, String strNewWindowName);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [Out] StringBuilder lParam);


		private IntPtr FindActiveWindow()
		{
			return GetForegroundWindow();
		}

		private const uint WM_GETTEXT = 0x000D;
		private const uint WM_GETTEXTLENGTH = 0x000E;

		public static string GetWindowTextRaw(IntPtr hwnd)
		{
			// Allocate correct string length first
			int length = (int)SendMessage(hwnd, WM_GETTEXTLENGTH, IntPtr.Zero, null);
			StringBuilder sb = new StringBuilder(length + 1);
			SendMessage(hwnd, WM_GETTEXT, (IntPtr)sb.Capacity, sb);
			return sb.ToString();
		}
		public void SetW(IntPtr handle, string text)
		{
			string current = GetWindowTextRaw(handle);

			int at = current.IndexOf('@');
			if (at > -1)
				current = current.Substring(0, at);

			if (!current.EndsWith(" ", StringComparison.OrdinalIgnoreCase))
				current += " ";

			SetWindowText(handle, current + "@" + text);
		}

		public WindowPath GetPathOfCurrentWindow()
		{
			IntPtr handle = FindActiveWindow();

			uint procId;
			GetWindowThreadProcessId(handle, out procId);

			var proc = Process.GetProcessById((int)procId);
			string processName = proc?.ProcessName;

			if (string.IsNullOrEmpty(processName))
				return null;

			var finder = _pathFinders.FirstOrDefault(f => f.CanHandle(processName));

			string path = finder?.FindPath(handle) ?? string.Empty;

			return new WindowPath() { Handle = handle, Path = path };
		}
	}

	public class WindowPath
	{
		public IntPtr Handle { get; set; }
		public string Path { get; set; }
	}
}
