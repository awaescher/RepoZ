using AppKit;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
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
		private static IRepositoryMonitor _repositoryMonitor;

		static void Main(string[] args)
		{
			var container = TinyIoCContainer.Current;

			RegisterServices(container);

			UseRepositoryMonitor(container);

			NSApplication.Init();
			NSApplication.Main(args);
		}

		static void RegisterServices(TinyIoCContainer container)
		{
			container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>().AsSingleton();

			container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>().AsSingleton();
			container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>().AsSingleton();
			container.Register<IPathCrawlerFactory, DefaultPathCrawlerFactory>().AsSingleton();

			container.Register<IErrorHandler, UIErrorHandler>();
			container.Register<IRepositoryActionProvider, MacRepositoryActionProvider>();
			container.Register<IRepositoryReader, DefaultRepositoryReader>();
			container.Register<IRepositoryWriter, DefaultRepositoryWriter>();
			container.Register<IRepositoryCache, MacRepositoryCache>();
			container.Register<IPathProvider, MacDriveEnumerator>();
			container.Register<IPathCrawler, GravellPathCrawler>();
			container.Register<IPathSkipper, MacPathSkipper>();
		}


		static void UseRepositoryMonitor(TinyIoCContainer container)
		{
            var uiContext = System.Threading.SynchronizationContext.Current;

            var repositoryInformationAggregator = container.Resolve<IRepositoryInformationAggregator>();
			_repositoryMonitor = container.Resolve<IRepositoryMonitor>();
            _repositoryMonitor.OnChangeDetected = (repo) => uiContext.Send(delegate { repositoryInformationAggregator.Add(repo); }, null);
            _repositoryMonitor.OnDeletionDetected = (repoPath) => uiContext.Send(delegate { repositoryInformationAggregator.RemoveByPath(repoPath); }, null);
			_repositoryMonitor.Observe();
		}
	}
}
