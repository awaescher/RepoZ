using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RepoZ.App.Win
{
	public static class AutoStart
	{
		private const string REG_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

		public static void SetStartup(string appName, bool startup)
		{
			var key = Registry.CurrentUser.OpenSubKey(REG_KEY, true);

			if (startup)
				key.SetValue(appName, GetAppPath());
			else
				key.DeleteValue(appName, false);
		}

		public static bool IsStartup(string appName)
		{
			var key = Registry.CurrentUser.OpenSubKey(REG_KEY);
			return IsStartup(key, appName);
		}

		public static bool IsStartup(RegistryKey key, string appName)
		{
			return GetValueAsString(key, appName)
					.Equals(GetAppPath(), StringComparison.OrdinalIgnoreCase);
		}

		private static string GetValueAsString(RegistryKey key, string appName)
		{
			var value = key.GetValue(appName);
			return value?.ToString() ?? "";
		}

		private static string GetAppPath() => $"\"{Assembly.GetEntryAssembly().Location}\"";
	}
}
