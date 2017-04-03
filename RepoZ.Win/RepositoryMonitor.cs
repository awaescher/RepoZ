using RepoZ.Shared;
using RepoZ.Win.Watchers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Win
{
	public class RepositoryMonitor
	{
		private ConcurrentDictionary<string, RepositoryHelper.RepositoryInfo> _repositories = new ConcurrentDictionary<string, RepositoryHelper.RepositoryInfo>();
		private List<IRepositoryWatcher> _watchers = new List<IRepositoryWatcher>();

		public RepositoryMonitor(params string[] paths)
		{
			IRepositoryHelper helper = new RepositoryHelper();
			foreach (var path in paths)
			{
				var watcher = new WindowsRepositoryWatcher(helper);
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

		private void OnRepositoryChangeDetected(RepositoryHelper.RepositoryInfo repo)
		{
			_repositories.AddOrUpdate(repo.Path, repo, (k, v) => repo);
			OnChangeDetected?.Invoke(repo);
		}

		public RepositoryHelper.RepositoryInfo[] Repositories => _repositories.Values.ToArray();

		public Action<RepositoryHelper.RepositoryInfo> OnChangeDetected { get; set; }
	}
}
