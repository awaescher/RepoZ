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

public class ActionAssociateFileV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IErrorHandler _errorHandler;

    public ActionAssociateFileV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionAssociateFileV1;
    }

    public bool CanHandleMultipeRepositories()
    {
        return false;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionAssociateFileV1, repository.First());
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionAssociateFileV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }


        var name = NameHelper.EvaluateName(action.Name, repository, _translationService);
        var command = _expressionEvaluator.EvaluateStringExpression(action.Command, repository);
        var arguments = action.Arguments; //todo

        yield return new Api.Git.RepositoryAction
            {
                Name = name,
                Action = (_, _) => ProcessHelper.StartProcess(command, arguments, _errorHandler),
            };
    }
}