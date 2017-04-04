using System;

namespace RepoZ.Api.Git
{
	public interface IRepositoryMonitor
	{
		Action<Repository> OnChangeDetected { get; set; }
		Repository[] Repositories { get; }

		void Stop();

		void Observe();
	}
}