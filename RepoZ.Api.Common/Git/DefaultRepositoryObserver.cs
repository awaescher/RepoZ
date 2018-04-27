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
		private bool _ioDetected;

		public Action<Repository> OnChange { get; set; }

		public void Setup(Repository repository, int detectionToAlertDelayMilliseconds)
		{
			DetectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

			_repository = repository;

			_watcher = new FileSystemWatcher(_repository.Path);
			_watcher.Created += _watcher_Created;
			_watcher.Changed += _watcher_Changed;
			_watcher.Deleted += _watcher_Deleted;
			_watcher.Renamed += _watcher_Renamed;
			_watcher.IncludeSubdirectories = true;
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
			PauseWatcherAndScheduleCallback();
		}

		private void _watcher_Renamed(object sender, RenamedEventArgs e)
		{
			PauseWatcherAndScheduleCallback();
		}

		private void _watcher_Changed(object sender, FileSystemEventArgs e)
		{
			PauseWatcherAndScheduleCallback();
		}

		private void _watcher_Created(object sender, FileSystemEventArgs e)
		{
			PauseWatcherAndScheduleCallback();
		}

		private void PauseWatcherAndScheduleCallback()
		{
			if (!_ioDetected)
			{
				_ioDetected = true;

				// stop the watcher once we found IO ...
				Stop();

				// ... and schedule a method to reactivate the watchers again
				// if nothing happened in between (regarding IO) it should also fire the OnChange-event
				Task.Run(() =>
					Thread.Sleep(DetectionToAlertDelayMilliseconds))
					.ContinueWith(AwakeWatcherAndScheduleEventInvocationIfNoFurtherIOGetsDetected);
			}
		}

		private void AwakeWatcherAndScheduleEventInvocationIfNoFurtherIOGetsDetected(object state)
		{
			if (_ioDetected)
			{
				// reset the flag, wait for further IO ...
				_ioDetected = false;
				Start();

				// ... and if nothing happened during the delay, invoke the OnChange-event
				Task.Run(() =>
					Thread.Sleep(DetectionToAlertDelayMilliseconds))
					.ContinueWith(t =>
					{
						if (_ioDetected)
							return;

						Console.WriteLine($"ONCHANGE on {_repository.Name}");
						OnChange?.Invoke(_repository);
					});
			}
		}

		public void Dispose()
		{
			if (_watcher != null)
			{
				_watcher.Created -= _watcher_Created;
				_watcher.Changed -= _watcher_Changed;
				_watcher.Deleted -= _watcher_Deleted;
				_watcher.Renamed -= _watcher_Renamed;
				_watcher.Dispose();
			}
		}

		public int DetectionToAlertDelayMilliseconds { get; private set; }
	}
}
