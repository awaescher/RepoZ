namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionExecutableV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly IErrorHandler _errorHandler;
    private readonly IFileSystem _fileSystem;

    public ActionExecutableV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, IErrorHandler errorHandler, IFileSystem fileSystem)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionExecutableV1;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionExecutableV1, repository);
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionExecutableV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = action.Name;
        // var name = ReplaceTranslatables(ReplaceVariables(_translationService.Translate(action.Name), repository));
        // var url = _expressionEvaluator.EvaluateStringExpression(action.Url, repository);

        var found = false;
        foreach (var executable in action.Executables)
        {
            if (found)
            {
                continue;
            }

            var normalized = _expressionEvaluator.EvaluateStringExpression(executable, repository);
            normalized = normalized.Replace("\"", "");

            if (!_fileSystem.File.Exists(normalized) && !_fileSystem.Directory.Exists(normalized))
            {
                continue;
            }

            var arguments = action.Arguments; //todo
            yield return new Api.Git.RepositoryAction
                {
                    Name = name,
                    Action = (_, _) => ProcessHelper.StartProcess(executable, arguments, _errorHandler),
                };
            found = true;
        }
    }
}