using System;
using RepoZ.Api.Git;

namespace RepoZ.Win
{
	public interface IRepositoryMonitor
	{
		Action<RepositoryInfo> OnChangeDetected { get; set; }
		RepositoryInfo[] Repositories { get; }

		void Stop();

		void Observe();
	}
}