using System;
using RepoZ.Api.Git;

namespace RepoZ.UI.Mac
{
	public class MacRepositoryMonitor : IRepositoryMonitor
	{
		public MacRepositoryMonitor()
		{
		}

		public Action<Repository> OnChangeDetected { get; set; }

		public Repository[] Repositories => new Repository[0];

		public void Observe()
		{
		}

		public void Stop()
		{
		}
	}
}
