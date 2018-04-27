using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	public interface IRepositoryActionProvider
	{
        RepositoryAction GetPrimaryAction(Repository repository);

        RepositoryAction GetSecondaryAction(Repository repository);

        IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories);
	}
}
