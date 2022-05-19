namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    private readonly ITranslationService _translationService;

    public ActionBrowserV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }

    bool IActionToRepositoryActionMapper.CanMap(Data.RepositoryAction action)
    {
        return action is RepositoryActionBrowserV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipeRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryAction> IActionToRepositoryActionMapper.Map(Data.RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionBrowserV1, repository.First());
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionBrowserV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        var url = _expressionEvaluator.EvaluateStringExpression(action.Url, repository);
        yield return new RepositoryAction()
            {
                Name = name,
                Action = (_, _) => ProcessHelper.StartProcess(url, string.Empty, _errorHandler),
            };
    }
}