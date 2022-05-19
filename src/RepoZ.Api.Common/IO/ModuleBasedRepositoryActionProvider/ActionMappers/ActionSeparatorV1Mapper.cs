namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionSeparatorV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;

    public ActionSeparatorV1Mapper(RepositoryExpressionEvaluator expressionEvaluator)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionSeparatorV1;
    }

    public bool CanHandleMultipeRepositories()
    {
        return true;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        foreach (Repository r in repository)
        {
            Api.Git.RepositoryAction[] result = Map(action as RepositoryActionSeparatorV1, r).ToArray();
            if (result.Any())
            {
                return result;
            }
        }

        return Array.Empty<Api.Git.RepositoryAction>();
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionSeparatorV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        yield return new Api.Git.RepositorySeparatorAction();
    }
}