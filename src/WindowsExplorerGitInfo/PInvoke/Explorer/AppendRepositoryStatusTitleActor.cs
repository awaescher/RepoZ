namespace RepoZ.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer
{
    using System;
    using RepoZ.Api.Git;

    internal class AppendRepositoryStatusTitleActor : ExplorerWindowActor
    {
        private readonly IRepositoryInformationAggregator _repositoryInfoAggregator;

        public AppendRepositoryStatusTitleActor(IRepositoryInformationAggregator repositoryInfoAggregator)
        {
            _repositoryInfoAggregator = repositoryInfoAggregator;
        }

        protected override void Act(IntPtr hwnd, string explorerLocationUrl)
        {
            if (string.IsNullOrEmpty(explorerLocationUrl))
            {
                return;
            }

            var path = new Uri(explorerLocationUrl).LocalPath;

            var status = _repositoryInfoAggregator.GetStatusByPath(path);

            if (string.IsNullOrEmpty(status))
            {
                return;
            }

            const string SEPARATOR = "  [";
            WindowHelper.AppendWindowText(hwnd, SEPARATOR, status + "]");
        }
    }
}