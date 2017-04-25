using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using System.Threading;

namespace RepoZ.Api.Win.Git
{
	public class DefaultRepositoryMonitor : IRepositoryMonitor
	{
		private Queue<Repository> _refreshQueue = new Queue<Repository>();
		private Timer _refreshTimer = null;
		private List<IRepositoryObserver> _observers = null;
		private IRepositoryObserverFactory _repositoryObserverFactory;
		private IPathCrawlerFactory _pathCrawlerFactory;
		private IRepositoryReader _repositoryReader;
		private IPathProvider _pathProvider;

		public DefaultRepositoryMonitor(IPathProvider pathProvider, IRepositoryReader repositoryReader, IRepositoryObserverFactory repositoryObserverFactory, IPathCrawlerFactory pathCrawlerFactory)
		{
			_repositoryReader = repositoryReader;
			_repositoryObserverFactory = repositoryObserverFactory;
			_pathCrawlerFactory = pathCrawlerFactory;
			_pathProvider = pathProvider;

			_refreshTimer = new Timer(RefreshTimerCallback, null, 1000, Timeout.Infinite);
		}

		private void ScanForRepositoriesAsync()
		{
			foreach (var path in _pathProvider.GetPaths().AsParallel())
			{
				var crawler = _pathCrawlerFactory.Create();
				Task.Run(() => crawler.Find(path, "HEAD", file => OnFoundNewRepository(file), null));
			}
		}

		private void OnFoundNewRepository(string file)
		{
			var repo = _repositoryReader.ReadRepository(file);
			if (repo.WasFound)
				OnRepositoryChangeDetected(repo);
		}

		private void OnCheckKnownRepository(string file)
		{
			var repo = _repositoryReader.ReadRepository(file);
			if (repo.WasFound)
				OnRepositoryChangeDetected(repo);
			else
				OnRepositoryDeletionDetected(file);
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

		private void RefreshTimerCallback(Object state)
		{
			if (_refreshQueue.Any())
			{
				var repo = _refreshQueue.Dequeue();
				OnCheckKnownRepository(repo.Path);
			}
			_refreshTimer.Change(1000, Timeout.Infinite);
		}

		public Action<Repository> OnChangeDetected { get; set; }
		public Action<string> OnDeletionDetected { get; set; }
	}
}
