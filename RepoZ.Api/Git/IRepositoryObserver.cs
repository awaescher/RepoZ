using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryObserver
	{
		void Setup(string path, int detectionToAlertDelayMs = 5000);

		void Observe();

		void Stop();

		Action<Repository> OnAddOrChange { get; set; }

		Action<string> OnDelete { get; set; }
	}
}
