using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Grr
{
	public static class ProcessExtensions
	{
		private static string FindIndexedProcessName(int pid)
		{
			var processName = Process.GetProcessById(pid).ProcessName;
			var processesByName = Process.GetProcessesByName(processName);
			string processIndexdName = null;

			for (var index = 0; index < processesByName.Length; index++)
			{
				processIndexdName = index == 0 ? processName : processName + "#" + index;
				var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
				if ((int)processId.NextValue() == pid)
					return processIndexdName;
			}

			return processIndexdName;
		}

		private static Process FindPidFromIndexedProcessName(string indexedProcessName)
		{
			var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
			return Process.GetProcessById((int)parentId.NextValue());
		}

		public static Process Parent(this Process process)
		{
			return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
		}
	}
}
