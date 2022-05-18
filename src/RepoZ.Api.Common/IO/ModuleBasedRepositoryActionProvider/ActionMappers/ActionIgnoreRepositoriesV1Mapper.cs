namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;

public class ActionIgnoreRepositoriesV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IErrorHandler _errorHandler;

    public ActionIgnoreRepositoriesV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    bool IActionToRepositoryActionMapper.CanMap(Data.RepositoryAction action)
    {
        return action is RepositoryActionIgnoreRepositoriesV1;
    }

    IEnumerable<RepositoryAction> IActionToRepositoryActionMapper.Map(Data.RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionIgnoreRepositoriesV1, repository);
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionIgnoreRepositoriesV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        // todo
        yield break;
    }
}