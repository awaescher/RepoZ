namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoZ.Api.Git;

public interface IActionToRepositoryActionMapper
{
    bool CanMap(RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction action);

    IEnumerable<RepositoryAction> Map(Data.RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition);
}