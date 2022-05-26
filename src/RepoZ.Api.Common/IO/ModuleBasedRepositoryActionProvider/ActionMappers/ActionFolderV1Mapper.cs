namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionFolderV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IErrorHandler _errorHandler;

    public ActionFolderV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, [NotNull] ITranslationService translationService, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }
    
    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionFolderV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipeRepositories()
    {
        return false;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionFolderV1, repository.First(), actionMapperComposition);
    }

    private IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionFolderV1 action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var deferred = false;
        if (!string.IsNullOrWhiteSpace(action.IsDeferred))
        {
            deferred = _expressionEvaluator.EvaluateBooleanExpression(action.IsDeferred, repository);
        }

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);

        if (deferred)
        {
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
        else
        {
            yield return new Api.Git.RepositoryAction()
                {
                    Name = name,
                    CanExecute = true,
                    SubActions = action.Items
                                       .Where(x => _expressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
                                       .SelectMany(x => actionMapperComposition.Map(x, repository))
                                       .ToArray(),
                };
        }
    }
}