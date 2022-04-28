namespace RepoZ.Ipc
{
    using RepoZ.Api;
    using SimpleInjector;
    using Container = SimpleInjector.Container;

    public static class Bootstrapper
    {
        public static void Register(Container container)
        {
            //IRepositorySource
            container.Register<IIpcEndpoint, DefaultIpcEndpoint>(Lifestyle.Singleton);
            container.Register<IpcServer>(Lifestyle.Singleton);
            container.Collection.Append<IModule, RepozIpcServerModule>(Lifestyle.Singleton);
        }
    }
}