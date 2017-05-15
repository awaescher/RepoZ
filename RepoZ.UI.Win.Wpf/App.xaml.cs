using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using RepoZ.Api.Common;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Win.Git;
using RepoZ.Api.Win.IO;
using RepoZ.Api.Win.PInvoke.Explorer;
using TinyIoC;

namespace RepoZ.UI.Win.Wpf
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static Timer _explorerUpdateTimer;
		private static WindowsExplorerHandler _explorerHandler;
		private static IRepositoryMonitor _repositoryMonitor;

		[STAThread]
		public static void Main(string[] args)
		{
			bool noUI = args?.Any(arg => arg.EndsWith("-noui")) ?? false;
			var container = TinyIoCContainer.Current;

			var application = new App();
			application.InitializeComponent();

			RegisterServices(container);

			UseRepositoryMonitor(container, application.Dispatcher);
			UseExplorerHandler(container);

			if (noUI)
			{
				application.Run();
			}
			else
			{
				var form = container.Resolve<MainWindow>();
				application.Run(form);
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_explorerUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

			var explorerHandler = TinyIoCContainer.Current.Resolve<WindowsExplorerHandler>();
			explorerHandler.CleanTitles();

			base.OnExit(e);
		}

		protected static void RegisterServices(TinyIoCContainer container)
		{
			container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>().AsSingleton();

			container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>().AsSingleton();
			container.Register<WindowsExplorerHandler>().AsSingleton();
			container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>().AsSingleton();
			container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>().AsSingleton();

			container.Register<IErrorHandler, UIErrorHandler>();
			container.Register<IRepositoryActionProvider, WindowsRepositoryActionProvider>();
			container.Register<IRepositoryReader, WindowsRepositoryReader>();
			container.Register<IRepositoryWriter, WindowsRepositoryWriter>();
			container.Register<IRepositoryCache, WindowsRepositoryCache>();
			container.Register<IPathProvider, DefaultDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathSkipper, WindowsPathSkipper>();
		}

		protected static void UseRepositoryMonitor(TinyIoCContainer container, Dispatcher uiDispatcher)
		{
			if (uiDispatcher == null)
				throw new ArgumentNullException(nameof(uiDispatcher));

			var repositoryInformationAggregator = container.Resolve<IRepositoryInformationAggregator>();
			_repositoryMonitor = container.Resolve<IRepositoryMonitor>();
			_repositoryMonitor.OnChangeDetected = (repo) => uiDispatcher.Invoke(() => repositoryInformationAggregator.Add(repo));
			_repositoryMonitor.OnDeletionDetected = (repoPath) => uiDispatcher.Invoke(() => repositoryInformationAggregator.RemoveByPath(repoPath));
			_repositoryMonitor.Observe();
		}

		protected static void UseExplorerHandler(TinyIoCContainer container)
		{
			_explorerHandler = container.Resolve<WindowsExplorerHandler>();
			_explorerUpdateTimer = new Timer(RefreshTimerCallback, null, 1000, Timeout.Infinite);
		}

		protected static void RefreshTimerCallback(Object state)
		{
			_explorerHandler.UpdateTitles();
			_explorerUpdateTimer.Change(500, Timeout.Infinite);
		}
	}
}
