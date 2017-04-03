using RepoZ.Shared;
using RepoZ.Win.Watchers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Win.Crawlers;

namespace RepoZ.Win
{
	public class RepositoryMonitor
	{
		private ConcurrentDictionary<string, RepositoryReader.RepositoryInfo> _repositories = new ConcurrentDictionary<string, RepositoryReader.RepositoryInfo>();
		private List<IRepositoryWatcher> _watchers = new List<IRepositoryWatcher>();
		private Func<IRepositoryWatcher> _repositoryWatcherFactory;
		private Func<IPathCrawler> _pathCrawlerFactory;
		private IRepositoryReader _repositoryReader;

		public RepositoryMonitor(IPathProvider pathProvider, IRepositoryReader repositoryReader, Func<IRepositoryWatcher> repositoryWatcherFactory, Func<IPathCrawler> pathCrawlerFactory)
		{
			_repositoryReader = repositoryReader;
			_repositoryWatcherFactory = repositoryWatcherFactory;
			_pathCrawlerFactory = pathCrawlerFactory;

			var paths = pathProvider.GetPaths();

			ScanForRepositories(paths);
			WatchForRepositoryChanges(paths);
		}

		private void ScanForRepositories(string[] paths)
		{
			foreach (var path in paths.AsParallel())
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


		private void WatchForRepositoryChanges(string[] paths)
		{
			foreach (var path in paths)
			{
				var watcher = _repositoryWatcherFactory();
				_watchers.Add(watcher);

				watcher.OnChangeDetected = OnRepositoryChangeDetected;
				watcher.Setup(path);
			}
		}


		public void Watch()
		{
			_watchers.ForEach(w => w.Watch());
		}

		public void Stop()
		{
			_watchers.ForEach(w => w.Stop());
		}

		private void OnRepositoryChangeDetected(RepositoryReader.RepositoryInfo repo)
		{
			_repositories.AddOrUpdate(repo.Path, repo, (k, v) => repo);
			OnChangeDetected?.Invoke(repo);
		}

		public RepositoryReader.RepositoryInfo[] Repositories => _repositories.Values.ToArray();

		public Action<RepositoryReader.RepositoryInfo> OnChangeDetected { get; set; }
	}
}
