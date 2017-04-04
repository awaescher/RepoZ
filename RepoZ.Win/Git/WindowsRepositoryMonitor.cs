using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;
using RepoZ.Api.IO;

namespace RepoZ.Win.Git
{
	public class WindowsRepositoryMonitor : IRepositoryMonitor
	{
		private ConcurrentDictionary<string, RepositoryInfo> _repositories = new ConcurrentDictionary<string, RepositoryInfo>();
		private List<IRepositoryObserver> _observers = null;
		private Func<IRepositoryObserver> _repositoryObserverFactory;
		private Func<IPathCrawler> _pathCrawlerFactory;
		private IRepositoryReader _repositoryReader;
		private IPathProvider _pathProvider;

		public WindowsRepositoryMonitor(IPathProvider pathProvider, IRepositoryReader repositoryReader, Func<IRepositoryObserver> repositoryObserverFactory, Func<IPathCrawler> pathCrawlerFactory)
		{
			_repositoryReader = repositoryReader;
			_repositoryObserverFactory = repositoryObserverFactory;
			_pathCrawlerFactory = pathCrawlerFactory;
			_pathProvider = pathProvider;
		}

		private void ScanForRepositoriesAsync()
		{
			foreach (var path in _pathProvider.GetPaths().AsParallel())
			{
				var crawler = _pathCrawlerFactory();
				Task.Run(() => crawler.Find(path, "HEAD", file => onFound(file), null));
			}
		}

		private void onFound(string file)
		{
			var repo = _repositoryReader.ReadRepository(file);
			if (repo.WasFound)
				OnRepositoryChangeDetected(repo);
		}


		private void ObserveRepositoryChanges()
		{
			_observers = new List<IRepositoryObserver>();

			foreach (var path in _pathProvider.GetPaths())
			{
				var observer = _repositoryObserverFactory();
				_observers.Add(observer);

				observer.OnChangeDetected = OnRepositoryChangeDetected;
				observer.Setup(path);
			}
		}

		public void Observe()
		{
			if (_observers == null)
			{
				ScanForRepositoriesAsync();
				ObserveRepositoryChanges();
			}

			_observers.ForEach(w => w.Observe());
		}

		public void Stop()
		{
			_observers.ForEach(w => w.Stop());
		}

		private void OnRepositoryChangeDetected(RepositoryInfo repo)
		{
			_repositories.AddOrUpdate(repo.Path, repo, (k, v) => repo);
			OnChangeDetected?.Invoke(repo);
		}

		public RepositoryInfo[] Repositories => _repositories.Values.ToArray();

		public Action<RepositoryInfo> OnChangeDetected { get; set; }
	}
}
