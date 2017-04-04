using RepoZ.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Win.Crawlers;
using RepoZ.Api.Git;
using RepoZ.Api.IO;

namespace RepoZ.Win
{
	public class RepositoryMonitor
	{
		private ConcurrentDictionary<string, RepositoryInfo> _repositories = new ConcurrentDictionary<string, RepositoryInfo>();
		private List<IRepositoryWatcher> _watchers = null;
		private Func<IRepositoryWatcher> _repositoryWatcherFactory;
		private Func<IPathCrawler> _pathCrawlerFactory;
		private IRepositoryReader _repositoryReader;
		private IPathProvider _pathProvider;

		public RepositoryMonitor(IPathProvider pathProvider, IRepositoryReader repositoryReader, Func<IRepositoryWatcher> repositoryWatcherFactory, Func<IPathCrawler> pathCrawlerFactory)
		{
			_repositoryReader = repositoryReader;
			_repositoryWatcherFactory = repositoryWatcherFactory;
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


		private void WatchForRepositoryChanges()
		{
			_watchers = new List<IRepositoryWatcher>();

			foreach (var path in _pathProvider.GetPaths())
			{
				var watcher = _repositoryWatcherFactory();
				_watchers.Add(watcher);

				watcher.OnChangeDetected = OnRepositoryChangeDetected;
				watcher.Setup(path);
			}
		}

		public void Watch()
		{
			if (_watchers == null)
			{
				ScanForRepositoriesAsync();
				WatchForRepositoryChanges();
			}

			_watchers.ForEach(w => w.Watch());
		}

		public void Stop()
		{
			_watchers.ForEach(w => w.Stop());
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
