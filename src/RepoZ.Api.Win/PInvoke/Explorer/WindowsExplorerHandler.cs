namespace RepoZ.Api.Win.PInvoke.Explorer
{
    using RepoZ.Api.Git;

    public class WindowsExplorerHandler
    {
        private readonly IRepositoryInformationAggregator _repositoryInfoAggregator;

        public WindowsExplorerHandler(IRepositoryInformationAggregator repositoryInfoAggregator)
        {
            _repositoryInfoAggregator = repositoryInfoAggregator;
        }

        public void UpdateTitles()
        {
            var actor = new AppendRepositoryStatusTitleActor(_repositoryInfoAggregator);
            actor.Pulse();
        }

        public void CleanTitles()
        {
            var actor = new CleanWindowTitleActor();
            actor.Pulse();
        }
    }
}