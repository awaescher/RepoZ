using RepoZ.Api.Git;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryDetector : IRepositoryDetector, IDisposable
	{
		private const string HEAD_LOG_FILE = @".git\logs\HEAD";

		private FileSystemWatcher _watcher;
		private readonly IRepositoryReader _repositoryReader;

		public DefaultRepositoryDetector(IRepositoryReader repositoryReader)
		{
			_repositoryReader = repositoryReader;
		}

		public Action<Repository> OnAddOrChange { get; set; }
		public Action<string> OnDelete { get; set; }

		public void Setup(string path, int detectionToAlertDelayMilliseconds)
		{
			DetectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

			_watcher = new FileSystemWatcher(path);
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
			if (!IsHead(e.FullPath))
				return;

			NotifyHeadDeletion(e.FullPath);
		}

		private void _watcher_Renamed(object sender, RenamedEventArgs e)
		{
			if (!IsHead(e.OldFullPath))
				return;

			NotifyHeadDeletion(e.OldFullPath);
		}

		private void _watcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (!IsHead(e.FullPath))
				return;

			EatRepo(e.FullPath);
		}

		private void _watcher_Created(object sender, FileSystemEventArgs e)
		{
			if (!IsHead(e.FullPath))
				return;

			Task.Run(() => Task.Delay(DetectionToAlertDelayMilliseconds))
				.ContinueWith(t => EatRepo(e.FullPath));
		}

		private bool IsHead(string path)
		{
			int index = GetGitPathEndFromHeadFile(path);
			return index == (path.Length - HEAD_LOG_FILE.Length);
		}

		private string GetRepositoryPathFromHead(string headFile)
		{
			int end = GetGitPathEndFromHeadFile(headFile);

			if (end < 0)
				return string.Empty;

			return headFile.Substring(0, end);
		}

		private int GetGitPathEndFromHeadFile(string path) => path.IndexOf(HEAD_LOG_FILE, StringComparison.OrdinalIgnoreCase);

		private void EatRepo(string path)
		{
			var repo = _repositoryReader.ReadRepository(path);

			if (repo?.WasFound ?? false)
				OnAddOrChange?.Invoke(repo);
		}

		private void NotifyHeadDeletion(string headFile)
		{
			string path = GetRepositoryPathFromHead(headFile);
			if (!string.IsNullOrEmpty(path))
				OnDelete?.Invoke(path);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
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