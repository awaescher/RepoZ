namespace WindowsExplorerGitInfo.PInvoke.Explorer
{
    using System;

    internal class CleanWindowTitleActor : ExplorerWindowActor
    {
        protected override void Act(IntPtr hwnd, string explorerLocationUrl)
        {
            Console.WriteLine("Clean " + explorerLocationUrl);
            const string SEPARATOR = "  [";
            WindowHelper.RemoveAppendedWindowText(hwnd, SEPARATOR);
        }
    }
}