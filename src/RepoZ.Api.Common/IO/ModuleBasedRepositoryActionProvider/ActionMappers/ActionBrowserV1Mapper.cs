namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using JetBrains.Annotations;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Git.RepositoryAction;

public class ActionBrowserV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly IErrorHandler _errorHandler;

    public ActionBrowserV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, [NotNull] IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    bool IActionToRepositoryActionMapper.CanMap(Data.RepositoryAction action)
    {
        return action is RepositoryActionBrowserV1;
    }

    IEnumerable<RepositoryAction> IActionToRepositoryActionMapper.Map(Data.RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionBrowserV1, repository);
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionBrowserV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = action.Name;
        // var name = ReplaceTranslatables(ReplaceVariables(_translationService.Translate(action.Name), repository));
        var url = _expressionEvaluator.EvaluateStringExpression(action.Url, repository);
        yield return new RepositoryAction()
            {
                Name = action.Name,
                Action = (_, _) => ProcessHelper.StartProcess(name, url, _errorHandler),
            };
    }
}