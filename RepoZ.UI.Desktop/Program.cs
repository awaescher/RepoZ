using System;
using Eto;
using Eto.Forms;
using RepoZ.Api.Win.Git;
using RepoZ.Api.Win.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Win;
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
			container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>();
			container.Register<IRepositoryObserver, DefaultRepositoryObserver>();
			container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>();
			container.Register<IRepositoryReader, WindowsRepositoryReader>();
			container.Register<IPathProvider, DefaultDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>();
			container.Register<IPathActionProvider, WindowsPathActionProvider>();

		   var application = new Application(Platform.Detect);
			var mainForm = container.Resolve<MainForm>();
			application.Run(mainForm);
		}
	}
}
