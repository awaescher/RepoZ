using AppKit;
using Eto;
using Eto.Forms;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Mac.Git;
using RepoZ.Api.Mac.IO;
using TinyIoC;

namespace RepoZ.UI.Mac
{
	static class MainClass
	{
		static void Main(string[] args)
		{
			var container = TinyIoCContainer.Current;

			container.Register<MainForm>();
			container.Register<IRepositoryMonitor, MacRepositoryMonitor>();
			//container.Register<IRepositoryObserver, WindowsRepositoryObserver>();
			//container.Register<IRepositoryObserverFactory, WindowsRepositoryObserverFactory>();
			//container.Register<IRepositoryReader, WindowsRepositoryReader>();
			container.Register<IPathProvider, MacDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			//container.Register<IPathCrawlerFactory, WindowsPathCrawlerFactory>();
			container.Register<IPathNavigator, MacPathNavigator>();

			var application = new Application(Platform.Detect);
			var mainForm = container.Resolve<MainForm>();
			application.Run(mainForm);
		}
	}
}
