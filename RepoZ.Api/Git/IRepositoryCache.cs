using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryCache
	{
		IEnumerable<string> Get();

		void Set(IEnumerable<string> paths);
	}
}
