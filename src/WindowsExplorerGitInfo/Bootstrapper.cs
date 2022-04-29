namespace WindowsExplorerGitInfo;

using RepoZ.Api;
using SimpleInjector;
using WindowsExplorerGitInfo.PInvoke.Explorer;

public static class Bootstrapper
{
    public static void Register(Container container)
    {
        container.Register<WindowsExplorerHandler>(Lifestyle.Singleton);
        container.Collection.Append<IModule, WindowExplorerBarGitInfoModule>(Lifestyle.Singleton);
    }
}