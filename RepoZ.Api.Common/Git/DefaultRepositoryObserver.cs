using RepoZ.Api.Git;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryObserver : IRepositoryObserver, IDisposable
	{
		private Repository _repository;
		private FileSystemWatcher _watcher;
		private static Timer _eventRaisingTimer;

		public Action<Repository> OnChange { get; set; }
		
		public void Setup(Repository repository, int detectionToAlertDelayMilliseconds = 500)
		{
			DetectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

			_repository = repository;
			_eventRaisingTimer = new Timer(RaiseEventCallback, null, Timeout.Infinite, Timeout.Infinite);

			_watcher = new FileSystemWatcher(_repository.Path);
			_watcher.Created += _watcher_Created;
			_watcher.Changed += _watcher_Changed;
			_watcher.Deleted += _watcher_Deleted;
			_watcher.Renamed += _watcher_Renamed;
			_watcher.IncludeSubdirectories = true;
		}

		private void RaiseEventCallback(object state)
		{
			OnChange?.Invoke(_repository);
		}

		public void Start()
		{
			_watcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			_watcher.EnableRaisingEvents = false;
		}

		private void _watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			RaiseChangeEvent();
		}

		private void _watcher_Renamed(object sender, RenamedEventArgs e)
		{
			RaiseChangeEvent();
		}

		private void _watcher_Changed(object sender, FileSystemEventArgs e)
		{
			RaiseChangeEvent();
		}

		private void _watcher_Created(object sender, FileSystemEventArgs e)
		{
			RaiseChangeEvent();
		}

		private void RaiseChangeEvent()
		{
			// use that delay to prevent a lot of sequential writes 
			// when a lot file changes are detected
			_eventRaisingTimer.Change(DetectionToAlertDelayMilliseconds, Timeout.Infinite);
		}

		public void Dispose()
		{
			if (_watcher != null)
			{
				_watcher.Created -= _watcher_Created;
				_watcher.Changed -= _watcher_Changed;
				_watcher.Deleted -= _watcher_Deleted;
				_watcher.Renamed -= _watcher_Renamed;
				_watcher?.Dispose();
			}
		}

		public int DetectionToAlertDelayMilliseconds { get; private set; }
	}
}
