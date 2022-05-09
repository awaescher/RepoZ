namespace RepoZ.Plugin.WindowsExplorerGitInfo
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using RepoZ.Api;
    using RepoZ.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;

    internal class WindowExplorerBarGitInfoModule : IModule
    {
        private readonly Timer _explorerUpdateTimer;
        private readonly WindowsExplorerHandler _explorerHandler;

        public WindowExplorerBarGitInfoModule(WindowsExplorerHandler explorerHandler)
        {
            _explorerHandler = explorerHandler ?? throw new ArgumentNullException(nameof(explorerHandler));
            _explorerUpdateTimer = new Timer(RefreshTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public Task StartAsync()
        {
            _explorerUpdateTimer.Change(1000, Timeout.Infinite);
            // _explorerUpdateTimer = new Timer(RefreshTimerCallback, null, 1000, Timeout.Infinite);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _explorerUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _explorerHandler.CleanTitles();
            return Task.CompletedTask;
        }

        protected void RefreshTimerCallback(object state)
        {
            _explorerHandler.UpdateTitles();
            _explorerUpdateTimer.Change(500, Timeout.Infinite);
        }
    }
}