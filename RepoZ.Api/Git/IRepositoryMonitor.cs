using System;

namespace RepoZ.Api.Git
{
	public interface IRepositoryMonitor
	{
		Action<Repository> OnChangeDetected { get; set; }

		void Stop();

		void Observe();
	}
}