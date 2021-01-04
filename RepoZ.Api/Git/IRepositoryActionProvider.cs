using System.Collections.Generic;

namespace RepoZ.Api.Git
{
	public interface IRepositoryActionProvider
	{
		RepositoryAction GetPrimaryAction(Repository repository);

		RepositoryAction GetSecondaryAction(Repository repository);

		IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories);
	}
}
