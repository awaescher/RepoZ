namespace RepoZ.Api.Win.PInvoke.Explorer
{
    using RepoZ.Api.Git;
    using System;

    public class AppendRepositoryStatusTitleActor : ExplorerWindowActor
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

            var separator = "  [";
            WindowHelper.AppendWindowText(hwnd, separator, status + "]");
        }
    }
}