namespace RepoZ.Api.Git
{
    using System.Collections.Generic;

    public interface IRepositoryActionProvider
    {
        RepositoryAction GetPrimaryAction(Repository repository);

        RepositoryAction GetSecondaryAction(Repository repository);

        IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories);
    }
}