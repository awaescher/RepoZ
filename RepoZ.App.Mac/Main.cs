using AppKit;
using Foundation;
using System.Threading;

namespace RepoZ.App.Mac
{
	static class MainClass
	{
		static void Main(string[] args)
		{
			NSApplication.Init();
			SetCurrentCulture();
			NSApplication.Main(args);
		}

		private static void SetCurrentCulture()
		{
			if ((NSLocale.PreferredLanguages?.Length ?? 0) == 0)
				return;

			Thread.CurrentThread.CurrentCulture
				= Thread.CurrentThread.CurrentUICulture
				= new System.Globalization.CultureInfo(NSLocale.PreferredLanguages[0]);
		}
	}
}
