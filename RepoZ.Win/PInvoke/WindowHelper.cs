using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Win.PInvoke
{
	public class WindowHelper
	{
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
		private static extern bool SetWindowTextApi(IntPtr hWnd, String strNewWindowName);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [Out] StringBuilder lParam);

		public static IntPtr FindActiveWindow()
		{
			return GetForegroundWindow();
		}

		private const uint WM_GETTEXT = 0x000D;
		private const uint WM_GETTEXTLENGTH = 0x000E;

		public static string GetWindowText(IntPtr hwnd)
		{
			// Allocate correct string length first
			int length = (int)SendMessage(hwnd, WM_GETTEXTLENGTH, IntPtr.Zero, null);
			StringBuilder sb = new StringBuilder(length + 1);
			SendMessage(hwnd, WM_GETTEXT, (IntPtr)sb.Capacity, sb);
			return sb.ToString();
		}

		public static void SetWindowText(IntPtr handle, string text)
		{
			SetWindowTextApi(handle, text);
		}

		public static void AppendWindowText(IntPtr handle, string uniqueSplitter, string text)
		{
			string current = GetWindowText(handle);

			int at = current.IndexOf(uniqueSplitter, StringComparison.OrdinalIgnoreCase);
			if (at > -1)
				current = current.Substring(0, at);

			//if (!current.EndsWith(" "))
			//	current += " ";

			SetWindowTextApi(handle, current + uniqueSplitter + text);
		}

	}

}
