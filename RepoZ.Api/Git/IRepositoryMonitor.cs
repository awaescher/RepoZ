using System;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryMonitor
	{
		event EventHandler<Repository> OnChangeDetected;

		event EventHandler<string> OnDeletionDetected;

		event EventHandler<bool> OnScanStateChanged;

		void Stop();

		void Observe();

		void Reset();

		Task ScanForLocalRepositoriesAsync();

		void IgnoreByPath(string path);
	}
}