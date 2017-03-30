using branch.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace branch.Win.Watchers
{
	public interface IRepositoryWatcher
	{
		void Setup(string path);

		void Watch();

		void Stop();

		Action<RepositoryHelper.RepositoryInfo> OnChangeDetected { get; set; }
	}
}
