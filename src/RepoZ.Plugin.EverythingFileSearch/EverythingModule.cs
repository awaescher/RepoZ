namespace RepoZ.Plugin.EverythingFileSearch
{
    using RepoZ.Api;
    using RepoZ.Api.IO;
    using SimpleInjector;
    using SimpleInjector.Packaging;

    public class EverythingModule : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Collection.Append<ISingleGitRepositoryFinderFactory, EverythingGitRepositoryFinderFactory>(Lifestyle.Singleton);
        }
    }
}