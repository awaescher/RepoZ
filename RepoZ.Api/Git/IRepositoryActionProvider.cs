using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryActionProvider
	{
		IEnumerable<RepositoryAction> GetFor(string path);
	}
}
