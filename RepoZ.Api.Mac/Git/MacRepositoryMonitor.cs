using System;
using RepoZ.Api.Git;

namespace RepoZ.Api.Mac.Git
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
