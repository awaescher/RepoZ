namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using System.IO.Abstractions;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoZ.Api.Git;

internal static class ActionMapperCompositionFactory
{
    public static ActionMapperComposition Create(
        RepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService,
        IErrorHandler errorHandler,
        IFileSystem fileSystem,
        IRepositoryWriter repositoryWriter,
        IRepositoryMonitor repositoryMonitor)
    {
        var list = new List<IActionToRepositoryActionMapper>
            {
                new ActionBrowseRepositoryV1Mapper(expressionEvaluator, translationService, errorHandler),
                new ActionBrowserV1Mapper(expressionEvaluator, translationService, errorHandler),
                new ActionCommandV1Mapper(expressionEvaluator, translationService, errorHandler),
                new ActionExecutableV1Mapper(expressionEvaluator,translationService, errorHandler, fileSystem),
                new ActionFolderV1Mapper(expressionEvaluator, translationService, errorHandler),
                new ActionGitCheckoutV1Mapper(expressionEvaluator, translationService, repositoryWriter, errorHandler),
                new ActionGitFetchV1Mapper(expressionEvaluator, translationService, repositoryWriter, errorHandler),
                new ActionGitPullV1Mapper(expressionEvaluator, translationService, repositoryWriter, errorHandler),
                new ActionGitPushV1Mapper(expressionEvaluator, translationService, repositoryWriter, errorHandler),
                new ActionIgnoreRepositoriesV1Mapper(expressionEvaluator, translationService, repositoryMonitor, errorHandler),
                new ActionSeparatorV1Mapper(expressionEvaluator),
                new ActionAssociateFileV1Mapper(expressionEvaluator, translationService, errorHandler),
            };

        return new ActionMapperComposition(list, expressionEvaluator);
    }
}