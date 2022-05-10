namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using RepoZ.Api.Git;

public class ModuleBasedRepositoryActionProvider : IRepositoryActionProvider
{
    public RepositoryAction GetPrimaryAction(Repository repository)
    {
        return null;
    }

    public RepositoryAction GetSecondaryAction(Repository repository)
    {
        return null;
    }

    public IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories)
    {
        yield break;
    }
}