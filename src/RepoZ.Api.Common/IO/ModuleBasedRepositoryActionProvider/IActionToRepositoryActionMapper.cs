namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoZ.Api.Git;

public interface IActionToRepositoryActionMapper
{
    bool CanMap(RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction action);

    bool CanHandleMultipeRepositories();

    IEnumerable<RepositoryAction> Map(Data.RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition);
}
