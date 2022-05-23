namespace RepoZ.Api.Common.Tests;

using System;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

internal static class DynamicRepositoryActionDeserializerFactory
{
    public static DynamicRepositoryActionDeserializer Create()
    {
        return new DynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new IActionDeserializer[]
                    {
                        new ActionExecutableV1Deserializer(),
                        new ActionCommandV1Deserializer(),
                        new ActionBrowserV1Deserializer(),
                        new ActionFolderV1Deserializer(),
                        new ActionSeparatorV1Deserializer(),
                        new ActionGitCheckoutV1Deserializer(),
                        new ActionGitFetchV1Deserializer(),
                        new ActionGitPushV1Deserializer(),
                        new ActionGitPullV1Deserializer(),
                        new ActionBrowseRepositoryV1Deserializer(),
                        new ActionIgnoreRepositoriesV1Deserializer(),
                        new ActionAssociateFileV1Deserializer(),
                    }));
    }

    public static DynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new DynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new IActionDeserializer[] { actionDeserializer, }));
    }
}