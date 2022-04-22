namespace RepoZ.Api.Win.PInvoke.Explorer
{
    using System;

    public class CleanWindowTitleActor : ExplorerWindowActor
    {
        protected override void Act(IntPtr hwnd, string explorerLocationUrl)
        {
            Console.WriteLine("Clean " + explorerLocationUrl);
            const string SEPARATOR = "  [";
            WindowHelper.RemoveAppendedWindowText(hwnd, SEPARATOR);
        }
    }
}