using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryObserver
	{
		void Setup(string path);

		void Observe();

		void Stop();

		Action<Repository> OnChangeDetected { get; set; }
	}
}
