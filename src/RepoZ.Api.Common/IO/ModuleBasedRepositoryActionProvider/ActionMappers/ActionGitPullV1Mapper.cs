namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionGitPullV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryWriter _repositoryWriter;
    private readonly IErrorHandler _errorHandler;

    public ActionGitPullV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IRepositoryWriter repositoryWriter, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionGitPullV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipeRepositories()
    {
        return true;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repositories, ActionMapperComposition actionMapperComposition)
    {
        Repository[] repos = repositories as Repository[] ?? repositories.ToArray();
        if (repos.Any(r => !_expressionEvaluator.EvaluateBooleanExpression(action.Active, r)))
        {
            yield break;
        }

        yield return MultipleRepositoryActionHelper.CreateActionForMultipleRepositories(
            _translationService.Translate("Pull"),
            repos,
            _repositoryWriter.Pull,
            executionCausesSynchronizing: true);
    }
}