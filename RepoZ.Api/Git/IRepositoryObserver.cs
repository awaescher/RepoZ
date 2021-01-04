using System;

namespace RepoZ.Api.Git
{
	public interface IRepositoryObserver : IDisposable
	{
		void Setup(Repository repository, int detectionToAlertDelayMilliseconds);

		void Start();

		void Stop();

		Action<Repository> OnChange { get; set; }
	}
}
