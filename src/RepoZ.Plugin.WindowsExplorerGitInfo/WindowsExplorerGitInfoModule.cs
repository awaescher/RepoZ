namespace RepoZ.Plugin.WindowsExplorerGitInfo;

using JetBrains.Annotations;
using RepoZ.Api;
using RepoZ.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class WindowsExplorerGitInfoModule : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Register<WindowsExplorerHandler>(Lifestyle.Singleton);
        container.Collection.Append<IModule, WindowExplorerBarGitInfoModule>(Lifestyle.Singleton);
    }
}