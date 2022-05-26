namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class ActionFolderV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "folder@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer)
    {
        return Deserialize(jToken, actionDeserializer);
    }

    public RepositoryActionFolderV1 Deserialize(JToken token, ActionDeserializerComposition actionDeserializer)
    {
        RepositoryActionFolderV1 result = token.ToObject<RepositoryActionFolderV1>();

        if (result == null)
        {
            return null;
        }

        JToken isDeferredToken = token["is-deferred"];

        if (isDeferredToken != null)
        {
            result.IsDeferred = isDeferredToken.Value<string>();
        }

        JToken actions = token.SelectToken("items");
        if (actions == null)
        {
            return result;
        }

        result.Items.Clear();

        IList<JToken> repositoryActions = actions.Children().ToList();
        foreach (JToken variable in repositoryActions)
        {
            JToken typeToken = variable["type"];
            if (typeToken == null)
            {
                continue;
            }

            var typeValue = typeToken.Value<string>();
            if (string.IsNullOrWhiteSpace(typeValue))
            {
                continue;
            }

            RepositoryAction customAction = actionDeserializer.DeserializeSingleAction(typeValue, variable);
            if (customAction == null)
            {
                continue;
            }
            
            result.Items.Add(customAction);
        }

        return result;
    }
}