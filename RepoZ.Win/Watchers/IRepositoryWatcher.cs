using RepoZ.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Win.Watchers
{
	public interface IRepositoryWatcher
	{
		void Setup(string path);

		void Watch();

		void Stop();

		Action<RepositoryReader.RepositoryInfo> OnChangeDetected { get; set; }
	}
}
