namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Git.RepositoryAction;

public class ActionMapperComposition
{
    private readonly IActionToRepositoryActionMapper[] _deserializers;
    private readonly RepositoryExpressionEvaluator _repoExpressionEvaluator;

    public ActionMapperComposition(IEnumerable<IActionToRepositoryActionMapper> deserializers, RepositoryExpressionEvaluator repoExpressionEvaluator)
    {
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _deserializers = deserializers?.Where(x => x != null).ToArray() ?? throw new ArgumentNullException(nameof(deserializers));
    }

    public IEnumerable<RepositoryAction> Map(RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction action, params RepoZ.Api.Git.Repository[] repositories)
    {
        Repository singleRepository = repositories.Length <= 1 ? repositories.SingleOrDefault() : null;
        
        List<Variable> EvaluateVariables(IEnumerable<Variable> vars)
        {
            if (vars == null)
            {
                return new List<Variable>(0);
            }

            if (singleRepository == null)
            {
                return new List<Variable>(0);
            }

            return vars
                   .Where(v => IsEnabled(v.Enabled, true, singleRepository))
                   .Select(v => new Variable()
                       {
                           Name = v.Name,
                           Enabled = "true",
                           Value = Evaluate(v.Value, singleRepository),
                       })
                   .ToList();
        }

        IActionToRepositoryActionMapper deserializer = _deserializers.FirstOrDefault(x => x.CanMap(action));

        using IDisposable disposable = RepoZVariableProviderStore.Push(EvaluateVariables(action.Variables));

        IEnumerable<RepositoryAction> result = deserializer?.Map(action, repositories, this);

        return result;

    }

    private string Evaluate(string input, Repository repository)
    {
        return _repoExpressionEvaluator.EvaluateStringExpression(input, repository);
    }

    private bool IsEnabled(string booleanExpression, bool defaultWhenNullOrEmpty, Repository repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _repoExpressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
    }
}