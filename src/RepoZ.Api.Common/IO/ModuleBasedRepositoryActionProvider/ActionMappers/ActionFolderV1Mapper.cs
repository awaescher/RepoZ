namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionFolderV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly IErrorHandler _errorHandler;

    public ActionFolderV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }
    
    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionFolderV1;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionFolderV1, repository, actionMapperComposition);
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionFolderV1 action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = action.Name;
        // var name = ReplaceTranslatables(ReplaceVariables(_translationService.Translate(action.Name), repository));

        yield return new Api.Git.RepositoryAction()
            {
                Name = name,
                CanExecute = true,
                DeferredSubActionsEnumerator = () =>
                    action.Items
                          .Where(x => _expressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
                          .SelectMany(x => actionMapperComposition.Map(x, repository))
                          .ToArray(),
            };
    }
}