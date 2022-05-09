namespace RepoZ.Plugin.LuceneSearch;

using JetBrains.Annotations;
using RepoZ.Api;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class LuceneSearchModule : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Register<IRepositorySearch, SearchAdapter>(Lifestyle.Singleton);
        container.Register<ILuceneDirectoryFactory, RamLuceneDirectoryFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryIndex, RepositoryIndex>(Lifestyle.Singleton);
        container.Register<LuceneDirectoryInstance, LuceneDirectoryInstance>(Lifestyle.Singleton);
        container.Register<RepositoryIndex, RepositoryIndex>(Lifestyle.Singleton);

        container.Collection.Append<IModule, EventToLuceneHandler>(Lifestyle.Singleton);
    }
}