using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IPersistentRepositoryCache
	{
		IEnumerable<string> Get();

		void Set(IEnumerable<string> paths);
	}
}
