using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryDetector
	{
		void Setup(string path, int detectionToAlertDelayMilliseconds);

		void Start();

		void Stop();

		Action<Repository> OnAddOrChange { get; set; }

		Action<string> OnDelete { get; set; }
	}
}
