namespace RepoZ.Plugin.IpcService
{
    using JetBrains.Annotations;
    using RepoZ.Api;
    using RepoZ.Ipc;
    using SimpleInjector;
    using SimpleInjector.Packaging;
    using Repository = RepoZ.Api.Git.Repository;

    [UsedImplicitly]
    public class IpcServiceModule : IPackage
    {
        public void RegisterServices(Container container)
        {
            //IRepositorySource
            container.Register<IIpcEndpoint, DefaultIpcEndpoint>(Lifestyle.Singleton);
            container.Register<IpcServer>(Lifestyle.Singleton);
            container.Collection.Append<IModule, RepozIpcServerModule>(Lifestyle.Singleton);
        }
    }
}