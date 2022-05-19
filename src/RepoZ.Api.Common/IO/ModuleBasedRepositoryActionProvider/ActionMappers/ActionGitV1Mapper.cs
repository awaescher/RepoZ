namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionGitV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly IErrorHandler _errorHandler;

    public ActionGitV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionGitV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipeRepositories()
    {
        return false;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionGitV1);
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionGitV1 action)
    {
        yield return new Api.Git.RepositoryAction()
            {
                Name = action.Name,
            };
    }
}