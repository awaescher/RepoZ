using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Grr
{
	public static class ConsoleExtensions
	{
		[DllImport("User32.dll")]
		static extern int SetForegroundWindow(IntPtr point);

		public static void WriteConsoleInput(Process target, string value)
		{
			SetForegroundWindow(target.MainWindowHandle);
			SendKeys.SendWait(value);
			SendKeys.SendWait("{Enter}");
		}
	}
}
