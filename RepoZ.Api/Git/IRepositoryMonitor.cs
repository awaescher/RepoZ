using System;

namespace RepoZ.Api.Git
{
	public interface IRepositoryMonitor
	{
		event EventHandler<Repository> OnChangeDetected;

		event EventHandler<string> OnDeletionDetected;

		void Stop();

		void Observe();
	}
}