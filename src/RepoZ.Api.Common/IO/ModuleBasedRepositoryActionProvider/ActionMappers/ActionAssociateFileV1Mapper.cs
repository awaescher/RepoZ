namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.IO;
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

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        var command = _expressionEvaluator.EvaluateStringExpression(action.Command, repository);

        //todo no arguments needed.
        // var arguments = _expressionEvaluator.EvaluateStringExpression(action.Arguments, repository);

        yield return CreateFileAssociationSubMenu(
            repository,
            name,
            action.Extension);
    }

    private Api.Git.RepositoryAction CreateProcessRunnerAction(string name, string process, string arguments = "")
    {
        return new Api.Git.RepositoryAction()
        {
            Name = name,
            Action = (_, _) => ProcessHelper.StartProcess(process, arguments, _errorHandler),
        };
    }

    private Api.Git.RepositoryAction CreateFileAssociationSubMenu(Repository repository, string actionName, string filePattern)
    {
        if (!HasFiles(repository, filePattern))
        {
            return null;
        }

        return new Api.Git.RepositoryAction()
            {
                Name = actionName,
                DeferredSubActionsEnumerator = () =>
                    GetFiles(repository, filePattern)
                        .Select(solutionFile => NameHelper.ReplaceVariables(solutionFile, repository))
                        .Select(solutionFile => CreateProcessRunnerAction(Path.GetFileName(solutionFile), solutionFile))
                        .ToArray(),
            };
    }

    private static bool HasFiles(Repository repository, string searchPattern)
    {
        return GetFileEnumerator(repository, searchPattern).Any();
    }

    private static IEnumerable<string> GetFiles(Repository repository, string searchPattern)
    {
        return GetFileEnumerator(repository, searchPattern)
               .OrderBy(f => f)
               .Take(25);
    }

    private static IEnumerable<string> GetFileEnumerator(Repository repository, string searchPattern)
    {
        // prefer EnumerateFileSystemInfos() over EnumerateFiles() to include packaged folders like
        // .app or .xcodeproj on macOS

        var directory = new DirectoryInfo(repository.Path);
        return directory
               .EnumerateFileSystemInfos(searchPattern, SearchOption.AllDirectories)
               .Select(f => f.FullName)
               .Where(f => !f.StartsWith("."));
    }
}