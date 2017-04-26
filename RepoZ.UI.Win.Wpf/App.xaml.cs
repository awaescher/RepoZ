using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
		private Timer _explorerUpdateTimer;
		private WindowsExplorerHandler _explorerHandler;
		private IRepositoryMonitor _repositoryMonitor;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var container = TinyIoCContainer.Current;

			container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>().AsSingleton();

			container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>().AsSingleton();
			container.Register<WindowsExplorerHandler>().AsSingleton();
			container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>().AsSingleton();
			container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>().AsSingleton();

			container.Register<IErrorHandler, UIErrorHandler>();
			container.Register<IRepositoryActionProvider, WindowsRepositoryActionProvider>();
			container.Register<IRepositoryReader, WindowsRepositoryReader>();
			container.Register<IRepositoryWriter, WindowsRepositoryWriter>();
			container.Register<IPathProvider, DefaultDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();

			var repositoryInformationAggregator = container.Resolve<IRepositoryInformationAggregator>();
			_repositoryMonitor = container.Resolve<IRepositoryMonitor>();
			_repositoryMonitor.OnChangeDetected = (repo) => Dispatcher.Invoke(() => repositoryInformationAggregator.Add(repo));
			_repositoryMonitor.OnDeletionDetected = (repoPath) => Dispatcher.Invoke(() => repositoryInformationAggregator.RemoveByPath(repoPath));
			_repositoryMonitor.Observe();

			_explorerHandler = container.Resolve<WindowsExplorerHandler>();

			_explorerUpdateTimer = new Timer(RefreshTimerCallback, null, 1000, Timeout.Infinite);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_explorerUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

			var explorerHandler = TinyIoCContainer.Current.Resolve<WindowsExplorerHandler>();
			explorerHandler.CleanTitles();

			base.OnExit(e);
		}

		private void RefreshTimerCallback(Object state)
		{
			_explorerHandler.UpdateTitles();
			_explorerUpdateTimer.Change(500, Timeout.Infinite);
		}
	}
}
