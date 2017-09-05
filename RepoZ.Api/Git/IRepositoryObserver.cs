using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryObserver
	{
		void Setup(Repository repository, int detectionToAlertDelayMilliseconds);

		void Start();

		void Stop();

		Action<Repository> OnChange { get; set; }
	}
}
