namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionCommandV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly IErrorHandler _errorHandler;

    public ActionCommandV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionCommandV1;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionCommandV1, repository);
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionCommandV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = action.Name;
        // var name = ReplaceTranslatables(ReplaceVariables(_translationService.Translate(action.Name), repository));

        var command = _expressionEvaluator.EvaluateStringExpression(action.Command, repository);
        var arguments = action.Arguments; //todo

        yield return new Api.Git.RepositoryAction
            {
                Name = action.Name,
                Action = (_, _) => ProcessHelper.StartProcess(command, arguments, _errorHandler),
            };
    }
}