using AppKit;
using Foundation;
using System.Globalization;
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

            try
            {
                Thread.CurrentThread.CurrentCulture
                    = Thread.CurrentThread.CurrentUICulture
                    = new CultureInfo(NSLocale.PreferredLanguages[0]);
            }
            catch (CultureNotFoundException)
            {
                // stick with english, then ...
            }

        }
    }
}
