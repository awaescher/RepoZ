namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public IEnumerable<RepositoryAction> Map(RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction action, params RepoZ.Api.Git.Repository[] repositories)
    {
        IActionToRepositoryActionMapper deserializer = _deserializers.FirstOrDefault(x => x.CanMap(action));

        using IDisposable disposable = RepoZVariableProviderStore.Push(action.Variables);
        
        IEnumerable<RepositoryAction> result = deserializer?.Map(action, repositories, this);

        return result;
    }
}