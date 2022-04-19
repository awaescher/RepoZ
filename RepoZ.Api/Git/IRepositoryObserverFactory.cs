namespace RepoZ.Api.Git
{
    public interface IRepositoryObserverFactory
    {
        IRepositoryObserver Create();
    }
}