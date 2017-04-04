using System;

namespace RepoZ.Api.Git
{
	public interface IRepositoryMonitor
	{
		Action<RepositoryInfo> OnChangeDetected { get; set; }
		RepositoryInfo[] Repositories { get; }

		void Stop();

		void Observe();
	}
}