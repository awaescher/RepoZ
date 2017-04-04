using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryWatcher
	{
		void Setup(string path);

		void Watch();

		void Stop();

		Action<RepositoryInfo> OnChangeDetected { get; set; }
	}
}
