using System;
using Eto;
using Eto.Forms;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Win;
using RepoZ.Win.Git;
using RepoZ.Win.IO;
using TinyIoC;

namespace RepoZ.UI.Desktop
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var container = TinyIoCContainer.Current;

			container.Register<MainForm>();
			container.Register<IRepositoryMonitor, WindowsRepositoryMonitor>();
			container.Register<IRepositoryObserver, WindowsRepositoryObserver>();
			container.Register<IRepositoryObserverFactory, WindowsRepositoryObserverFactory>();
			container.Register<IRepositoryReader, WindowsRepositoryReader>();
			container.Register<IPathProvider, WindowsDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathCrawlerFactory, WindowsPathCrawlerFactory>();
			container.Register<IPathNavigator, WindowsPathNavigator>();

			var application = new Application(Platform.Detect);
			var mainForm = container.Resolve<MainForm>();
			application.Run(mainForm);
		}
	}
}
