namespace RepoZ.Api.Common.Git
{
    using System.IO;
    using System.Threading.Tasks;
    using RepoZ.Api.Git;

    public interface IRepositoryActionConfigurationStore
    {
        string GetFileName();

        RepositoryActionConfiguration LoadGlobalRepositoryActions();

        RepositoryActionConfiguration LoadRepositoryConfiguration(Repository repo);

        RepositoryActionConfiguration LoadRepositoryActionConfiguration(string filename);

        RepositoryActionConfiguration LoadRepositoryActionConfigurationFromJson(string jsonContent);
    }
}