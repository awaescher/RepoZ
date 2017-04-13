using AppKit;
using Eto;
using Eto.Forms;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Mac;
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
			container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>();
			container.Register<IRepositoryObserver, DefaultRepositoryObserver>();
			container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>();
			container.Register<IRepositoryReader, MacRepositoryReader>();
			container.Register<IRepositoryWriter, MacRepositoryWriter>();
			container.Register<IRepositoryActionProvider, MacRepositoryActionProvider>();
			container.Register<IPathProvider, DefaultDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>();

			var application = new Application(Platform.Detect);
			var mainForm = container.Resolve<MainForm>();
			application.Run(mainForm);
		}
	}
}
