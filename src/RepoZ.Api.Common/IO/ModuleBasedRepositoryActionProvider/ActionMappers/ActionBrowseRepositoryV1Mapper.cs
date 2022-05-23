namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;

public class ActionBrowseRepositoryV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IErrorHandler _errorHandler;

    public ActionBrowseRepositoryV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    bool IActionToRepositoryActionMapper.CanMap(Data.RepositoryAction action)
    {
        return action is RepositoryActionBrowseRepositoryV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipeRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryAction> IActionToRepositoryActionMapper.Map(Data.RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionBrowseRepositoryV1, repository.First());
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionBrowseRepositoryV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        RepositoryAction result = CreateBrowseRemoteAction(repository, action);
        if (result != null)
        {
            yield return result;
        }
    }

    private RepositoryAction CreateBrowseRemoteAction(Repository repository, RepositoryActionBrowseRepositoryV1 action = null)
    {
        if (repository.RemoteUrls.Length == 0)
        {
            return null;
        }

        var forceSingle = false;
        if (!string.IsNullOrWhiteSpace(action?.FirstOnly))
        {
            forceSingle = _expressionEvaluator.EvaluateBooleanExpression(action.FirstOnly, repository);
        }

        var actionName = _translationService.Translate("Browse remote");

        if (repository.RemoteUrls.Length == 1 || forceSingle)
        {
            return CreateProcessRunnerAction(actionName, repository.RemoteUrls[0]);
        }

        return new RepositoryAction()
            {
                Name = actionName,
                DeferredSubActionsEnumerator = () => repository.RemoteUrls
                                                               .Take(50)
                                                               .Select(url => CreateProcessRunnerAction(url, url))
                                                               .ToArray(),
            };
    }

    private RepositoryAction CreateProcessRunnerAction(string name, string process, string arguments = "")
    {
        return new RepositoryAction
            {
                Name = name,
                Action = (_, _) => ProcessHelper.StartProcess(process, arguments, _errorHandler),
            };
    }
}