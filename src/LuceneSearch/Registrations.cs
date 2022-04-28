namespace LuceneSearch;

using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Antlr.Runtime.Misc;
using RepoZ.Api;
using SimpleInjector;

public static class Registrations
{
    public static void Register(Container container)
    {
        container.Register<IRepositorySearch, SearchAdapter>(Lifestyle.Singleton);
        container.Register<ILuceneDirectoryFactory, RamLuceneDirectoryFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryIndex, RepositoryIndex>(Lifestyle.Singleton);
        container.Register<LuceneDirectoryInstance, LuceneDirectoryInstance>(Lifestyle.Singleton);
        container.Register<RepositoryIndex, RepositoryIndex>(Lifestyle.Singleton);

        container.Collection.Append<IModule, EventToLuceneHandler>(Lifestyle.Singleton);
    }
}