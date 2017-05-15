using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using System.Threading;
using RepoZ.Api.Common;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryMonitor : IRepositoryMonitor
	{
		private SingularityQueue<Repository> _refreshQueue = new SingularityQueue<Repository>();
		private Timer _refreshTimer = null;
		private Timer _cacheFlushTimer = null;
		private List<IRepositoryObserver> _observers = null;
		private IRepositoryObserverFactory _repositoryObserverFactory;
		private IPathCrawlerFactory _pathCrawlerFactory;
		private IRepositoryReader _repositoryReader;
		private IPathProvider _pathProvider;
		private IRepositoryCache _repositoryCache;
		private IRepositoryInformationAggregator _repositoryInformationAggregator;

		public DefaultRepositoryMonitor(IPathProvider pathProvider, IRepositoryReader repositoryReader, IRepositoryObserverFactory repositoryObserverFactory, IPathCrawlerFactory pathCrawlerFactory, IRepositoryCache repositoryCache, IRepositoryInformationAggregator repositoryInformationAggregator)
		{
			_repositoryReader = repositoryReader;
			_repositoryObserverFactory = repositoryObserverFactory;
			_pathCrawlerFactory = pathCrawlerFactory;
			_pathProvider = pathProvider;
			_repositoryCache = repositoryCache;
			_repositoryInformationAggregator = repositoryInformationAggregator;

			_refreshTimer = new Timer(RefreshTimerCallback, null, 1000, Timeout.Infinite);
			_cacheFlushTimer = new Timer(RepositoryCacheFlushTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
		}

		private void ScanForRepositoriesAsync()
		{
			Scanning = true;
			OnScanStateChanged?.Invoke(Scanning);

			int scannedPaths = 0;

			var paths = _pathProvider.GetPaths();

			foreach (var path in paths.AsParallel())
			{
				var crawler = _pathCrawlerFactory.Create();
				Task.Run(() => crawler.Find(path, "HEAD", file => OnFoundNewRepository(file), null))
					.ContinueWith((t) => scannedPaths++)
					.ContinueWith((t) =>
					{
						bool newScanningState = (scannedPaths < paths.Length);
						bool didChange = newScanningState != Scanning;
						Scanning = newScanningState;

						if (didChange)
							OnScanStateChanged?.Invoke(Scanning);
					});
			}
		}

		private void ScanCachedRepositoriesAsync()
		{
			Task.Run(() =>
			{
				foreach (var head in _repositoryCache.Get())
					OnCheckKnownRepository(head, KnownRepositoryNotification.WhenFound);
			});
		}

		private void RepositoryCacheFlushTimerCallback(object state)
		{
			var heads = _repositoryInformationAggregator.Repositories.Select(v => v.Path).ToArray();
			_repositoryCache.Set(heads);
		}

		private void OnFoundNewRepository(string file)
		{
			var repo = _repositoryReader.ReadRepository(file);
			if (repo.WasFound)
			{
				OnRepositoryChangeDetected(repo);

				// use that delay to prevent a lot of sequential writes 
				// when a lot repositories get found in a row
				_cacheFlushTimer.Change(5000, Timeout.Infinite);
			}
		}

		private void OnCheckKnownRepository(string file, KnownRepositoryNotification notification)
		{
			var repo = _repositoryReader.ReadRepository(file);
			if (repo.WasFound)
			{
				if (notification.HasFlag(KnownRepositoryNotification.WhenFound))
					OnRepositoryChangeDetected(repo);
			}
			else
			{
				if (notification.HasFlag(KnownRepositoryNotification.WhenNotFound))
					OnRepositoryDeletionDetected(file);
			}
		}

		private void ObserveRepositoryChanges()
		{
			_observers = new List<IRepositoryObserver>();

			foreach (var path in _pathProvider.GetPaths())
			{
				var observer = _repositoryObserverFactory.Create();
				_observers.Add(observer);

				observer.OnAddOrChange = OnRepositoryChangeDetected;
				observer.OnDelete = OnRepositoryDeletionDetected;
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

			ScanCachedRepositoriesAsync();
			_observers.ForEach(w => w.Observe());
		}

		public void Stop()
		{
			_observers.ForEach(w => w.Stop());
		}

		private void OnRepositoryChangeDetected(Repository repo)
		{
			OnChangeDetected?.Invoke(repo);
			_refreshQueue.Enqueue(repo);
		}

		private void OnRepositoryDeletionDetected(string repoPath)
		{
			OnDeletionDetected?.Invoke(repoPath);
		}

		private void RefreshTimerCallback(object state)
		{
			if (!Scanning && _refreshQueue.Any())
			{
				var repo = _refreshQueue.Dequeue();
				OnCheckKnownRepository(repo.Path, KnownRepositoryNotification.WhenFound | KnownRepositoryNotification.WhenNotFound);
			}
			_refreshTimer.Change(2000, Timeout.Infinite);
		}

		public Action<Repository> OnChangeDetected { get; set; }

		public Action<string> OnDeletionDetected { get; set; }

		public Action<bool> OnScanStateChanged { get; set; }

		public bool Scanning = false;

		private enum KnownRepositoryNotification
		{
			WhenFound = 1,
			WhenNotFound = 2
		}
	}
}
