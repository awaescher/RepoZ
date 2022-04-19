namespace RepoZ.Api.Common.Git
{
    using RepoZ.Api.Git;

    public interface IRepositoryActionConfigurationStore
    {
        void Preload();

        RepositoryActionConfiguration RepositoryActionConfiguration { get; }

        RepositoryActionConfiguration LoadRepositoryConfiguration(Repository repo);

        RepositoryActionConfiguration LoadRepositoryActionConfiguration(string filename);
    }
}