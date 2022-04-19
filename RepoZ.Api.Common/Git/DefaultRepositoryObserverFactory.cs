namespace RepoZ.Api.Common.Git
{
    using RepoZ.Api.Git;

    public class DefaultRepositoryObserverFactory : IRepositoryObserverFactory
    {
        public IRepositoryObserver Create()
        {
            return new DefaultRepositoryObserver();
        }
    }
}