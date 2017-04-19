using System;
using Eto;
using Eto.Forms;
using RepoZ.Api.Win.Git;
using RepoZ.Api.Win.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Win;
using TinyIoC;
using RepoZ.Api.Win.PInvoke;

namespace RepoZ.UI.Win
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var container = TinyIoCContainer.Current;

			container.Register<MainForm>();
			container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>();
			container.Register<IRepositoryObserver, DefaultRepositoryObserver>();
			container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>();
			container.Register<IRepositoryReader, WindowsRepositoryReader>();
			container.Register<IRepositoryWriter, WindowsRepositoryWriter>();
			container.Register<IPathProvider, DefaultDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>();
			container.Register<IRepositoryActionProvider, WindowsRepositoryActionProvider>();
			container.Register<IRepositoryInformationAggregator, MainForm>();
			container.Register<WindowsExplorerHandler>();

			var application = new Application(Platform.Detect);
			var mainForm = container.Resolve<MainForm>();

			var handler = new WindowsExplorerHandler(mainForm);
			var timer = new UITimer();
			timer.Interval = 0.5;
			timer.Elapsed += (s, e) => handler.Pulse();
			timer.Start();

			application.Run(mainForm);
		}
	}
}
