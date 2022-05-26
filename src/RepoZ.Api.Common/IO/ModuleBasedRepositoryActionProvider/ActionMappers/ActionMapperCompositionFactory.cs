namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System.Collections.Generic;
using System.IO.Abstractions;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Git;

public static class ActionMapperCompositionFactory
{
    // test purposes
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
                new ActionAssociateFileV1Mapper(expressionEvaluator, translationService, errorHandler)
            };

        return new ActionMapperComposition(list, expressionEvaluator);
    }
}