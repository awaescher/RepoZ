namespace RepoZ.Api.Common.Git
{
    using System.IO;
    using System.Threading.Tasks;
    using RepoZ.Api.Git;

    public interface IRepositoryActionConfigurationStore
    {
        void Preload();

        RepositoryActionConfiguration RepositoryActionConfiguration { get; }

        RepositoryActionConfiguration LoadRepositoryConfiguration(Repository repo);

        RepositoryActionConfiguration LoadRepositoryActionConfiguration(string filename);

        RepositoryActionConfiguration LoadRepositoryActionConfigurationFromJson(string jsonContent);

        Task<RepositoryActionConfiguration> LoadRepositoryActionConfiguration(Stream stream);
    }
}