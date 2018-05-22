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
using System.IO;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryMonitor : IRepositoryMonitor
	{
		public event EventHandler<Repository> OnChangeDetected;
		public event EventHandler<string> OnDeletionDetected;
		public event EventHandler<bool> OnScanStateChanged;

		private Timer _storeFlushTimer = null;
		private List<IRepositoryDetector> _detectors = null;
		private IRepositoryDetectorFactory _repositoryDetectorFactory;
		private IRepositoryObserverFactory _repositoryObserverFactory;
		private IPathCrawlerFactory _pathCrawlerFactory;
		private IRepositoryReader _repositoryReader;
		private IPathProvider _pathProvider;
		private IRepositoryStore _repositoryStore;
		private IRepositoryInformationAggregator _repositoryInformationAggregator;
		private Dictionary<string, IRepositoryObserver> _repositoryObservers;

		public DefaultRepositoryMonitor(
			IPathProvider pathProvider,
			IRepositoryReader repositoryReader,
			IRepositoryDetectorFactory repositoryDetectorFactory,
			IRepositoryObserverFactory repositoryObserverFactory,
			IPathCrawlerFactory pathCrawlerFactory,
			IRepositoryStore repositoryStore,
			IRepositoryInformationAggregator repositoryInformationAggregator)
		{
			_repositoryReader = repositoryReader;
			_repositoryDetectorFactory = repositoryDetectorFactory;
			_repositoryObserverFactory = repositoryObserverFactory;
			_pathCrawlerFactory = pathCrawlerFactory;
			_pathProvider = pathProvider;
			_repositoryStore = repositoryStore;
			_repositoryInformationAggregator = repositoryInformationAggregator;

			_repositoryObservers = new Dictionary<string, IRepositoryObserver>();

			_storeFlushTimer = new Timer(RepositoryStoreFlushTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
		}

		public Task ScanForLocalRepositoriesAsync()
		{
			Scanning = true;
			OnScanStateChanged?.Invoke(this, Scanning);

			int scannedPaths = 0;

			var paths = _pathProvider.GetPaths();

			var tasks = paths.Select(path =>
			{
				return Task.Run(() => _pathCrawlerFactory.Create().Find(path, "HEAD", OnFoundNewRepository, null))
					.ContinueWith(t => scannedPaths++)
					.ContinueWith(t =>
					{
						bool newScanningState = (scannedPaths < paths.Length);
						bool didChange = newScanningState != Scanning;
						Scanning = newScanningState;

						if (didChange)
							OnScanStateChanged?.Invoke(this, Scanning);
					});
			});

			return Task.WhenAll(tasks);
		}

		private void ScanRepositoriesFromStoreAsync()
		{
			Task.Run(() =>
			{
				foreach (var head in _repositoryStore.Get())
					OnCheckKnownRepository(head, KnownRepositoryNotification.WhenFound);
			});
		}

		private void RepositoryStoreFlushTimerCallback(object state)
		{
			var heads = _repositoryInformationAggregator.Repositories.Select(v => v.Path).ToArray();
			_repositoryStore.Set(heads);
		}

		private void OnFoundNewRepository(string file)
		{
			var repo = _repositoryReader.ReadRepository(file);
			if (repo.WasFound)
			{
				OnRepositoryChangeDetected(repo);

				// use that delay to prevent a lot of sequential writes 
				// when a lot repositories get found in a row
				_storeFlushTimer.Change(5000, Timeout.Infinite);
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
			_detectors = new List<IRepositoryDetector>();

			foreach (var path in _pathProvider.GetPaths())
			{
				if (!Directory.Exists(path))
					continue;

				var detector = _repositoryDetectorFactory.Create();
				_detectors.Add(detector);

				detector.OnAddOrChange = OnRepositoryChangeDetected;
				detector.OnDelete = OnRepositoryDeletionDetected;
				detector.Setup(path, DelayGitRepositoryStatusAfterCreationMilliseconds);
			}
		}

		public void Observe()
		{
			if (_detectors == null)
			{
				// see https://answers.unity.com/questions/959106/how-to-monitor-file-system-in-mac.html
				Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");

				ScanRepositoriesFromStoreAsync();

				ObserveRepositoryChanges();
			}

			_detectors.ForEach(w => w.Start()); // TODO EXC_BAD_ACCESS (SIGABRT) on Mac?!
		}


		public void Stop()
		{
			_detectors?.ForEach(w => w.Stop());
		}

		public void Reset()
		{
			Stop();
			_detectors = null;
		}

		private void OnRepositoryChangeDetected(Repository repo)
		{
			string path = repo?.Path;

			if (string.IsNullOrEmpty(path))
				return;

			if (!_repositoryInformationAggregator.HasRepository(path))
				CreateRepositoryObserver(repo, path);

			OnChangeDetected?.Invoke(this, repo);

			_repositoryInformationAggregator.Add(repo);

		}

		private void CreateRepositoryObserver(Repository repo, string path)
		{
			var observer = _repositoryObserverFactory.Create();
			observer.Setup(repo, DelayGitStatusAfterFileOperationMilliseconds);
			_repositoryObservers.Add(path, observer);

			observer.OnChange += OnRepositoryObserverChange;
			observer.Start();
		}

		private void OnRepositoryObserverChange(Repository repository)
		{
			OnCheckKnownRepository(repository.Path, KnownRepositoryNotification.WhenFound | KnownRepositoryNotification.WhenNotFound);
		}

		private void DestroyRepositoryObserver(string path)
		{
			if (_repositoryObservers.TryGetValue(path, out IRepositoryObserver observer))
			{
				observer.Stop();
				_repositoryObservers.Remove(path);
			}
		}

		private void OnRepositoryDeletionDetected(string repoPath)
		{
			if (string.IsNullOrEmpty(repoPath))
				return;

			DestroyRepositoryObserver(repoPath);

			OnDeletionDetected?.Invoke(this, repoPath);

			_repositoryInformationAggregator.RemoveByPath(repoPath);
		}

		public bool Scanning { get; set; } = false;

		public int DelayGitRepositoryStatusAfterCreationMilliseconds { get; set; } = 5000;

		public int DelayGitStatusAfterFileOperationMilliseconds { get; set; } = 500;

		private enum KnownRepositoryNotification
		{
			WhenFound = 1,
			WhenNotFound = 2
		}
	}
}
