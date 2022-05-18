namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepositoryAction = RepoZ.Api.Git.RepositoryAction;

public class ActionMapperComposition
{
    private readonly IActionToRepositoryActionMapper[] _deserializers;

    public ActionMapperComposition(IEnumerable<IActionToRepositoryActionMapper> deserializers)
    {
        _deserializers = deserializers?.Where(x => x != null).ToArray() ?? throw new ArgumentNullException(nameof(deserializers));
    }

    public IEnumerable<RepositoryAction> Map(RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction action, RepoZ.Api.Git.Repository repository)
    {
        IActionToRepositoryActionMapper deserializer = _deserializers.FirstOrDefault(x => x.CanMap(action));

        IEnumerable<RepositoryAction> result = deserializer?.Map(action, repository, this);

        return result;
    }
}