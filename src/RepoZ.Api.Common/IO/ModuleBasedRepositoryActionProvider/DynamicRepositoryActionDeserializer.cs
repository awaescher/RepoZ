namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

public class DynamicRepositoryActionDeserializer
{
    private readonly List<IActionDeserializer> _deserializers = new()
        {
            new ActionBrowserV1Deserializer(),
            new FolderV1Deserializer(),
        };

    public RepositoryActionConfiguration2 Deserialize(ReadOnlySpan<char> rawContent)
    {
        var configuration = new RepositoryActionConfiguration2();

        var value = rawContent.ToString();

        var jsonObject = JObject.Parse(
            value,
            new JsonLoadSettings()
                {
                    CommentHandling = CommentHandling.Ignore,
                });

        JToken token = jsonObject["redirect"];
        configuration.Redirect = DeserializeRedirect(token);

        token = jsonObject["repository-specific-env-files"];
        configuration.RepositorySpecificEnvironmentFiles.AddRange(TryDeserializeEnumerable<FileReference>(token));

        token = jsonObject["repository-specific-config-files"];
        configuration.RepositorySpecificConfigFiles.AddRange(TryDeserializeEnumerable<FileReference>(token));

        token = jsonObject["variables"];
        configuration.Variables.AddRange(TryDeserializeEnumerable<Variable>(token));

        token = jsonObject["repository-tags"];
        DeserializeRepositoryTags(token, ref configuration);

        token = jsonObject["repository-actions"];
        DeserializeRepositoryActions(token, configuration);

        return configuration;
    }

    private void DeserializeRepositoryActions(JToken repositoryActionsToken, RepositoryActionConfiguration2 configuration)
    {
        if (repositoryActionsToken == null)
        {
            return;
        }

        // check if it is style 2
        JToken actions = repositoryActionsToken.SelectToken("actions");
        if (actions != null)
        {
            JToken jTokenVariables = repositoryActionsToken.SelectToken("variables");
            configuration.ActionsCollection.Variables.AddRange(TryDeserializeEnumerable<Variable>(jTokenVariables));
            repositoryActionsToken = actions;
        }

        // assume style 1
        IList<JToken> repositoryActions = repositoryActionsToken.Children().ToList();
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

            RepositoryAction customAction = DeserializeAction(typeValue, variable);
            if (customAction == null)
            {
                continue;
            }

            configuration.ActionsCollection.Actions.Add(customAction);
        }
    }

    private static void DeserializeRepositoryTags(JToken repositoryTagsToken, ref RepositoryActionConfiguration2 configuration)
    {
        if (repositoryTagsToken == null)
        {
            return;
        }

        // check style
        JToken tagsToken = repositoryTagsToken.SelectToken("tags");
        if (tagsToken != null)
        {
            // st
            JToken token = repositoryTagsToken.SelectToken("variables");
            configuration.TagsCollection.Variables.AddRange(TryDeserializeEnumerable<Variable>(token));
            configuration.TagsCollection.Tags.AddRange(TryDeserializeEnumerable<RepositoryActionTag>(tagsToken));
        }
        else
        {
            // assume style 1
            configuration.TagsCollection.Tags.AddRange(TryDeserializeEnumerable<RepositoryActionTag>(repositoryTagsToken));
        }
    }

    private static IEnumerable<T> TryDeserializeEnumerable<T>(JToken token)
    {
        if (token == null)
        {
            yield break;
        }

        IList<JToken> files = token.Children().ToList();
        foreach (JToken file in files)
        {
            T v = file.ToObject<T>();
            if (v != null)
            {
                yield return v;
            }
        }
    }

    private static Redirect DeserializeRedirect(JToken redirectToken)
    {
        if (redirectToken == null)
        {
            return null;
        }

        if (redirectToken.Type != JTokenType.String)
        {
            return redirectToken.ToObject<Redirect>();
        }

        var value = redirectToken.Value<string>();
        return new Redirect()
            {
                Filename = value,
                // Enabled = 
            };

    }

    private RepositoryAction DeserializeAction(string type, JToken obj)
    {
        IActionDeserializer instance = _deserializers.FirstOrDefault(x => x.CanDeserialize(type));
        RepositoryAction result = instance == null
            ? obj.ToObject<RepositoryAction>()
            : instance.Deserialize(obj);

        if (result == null)
        {
            return null;
        }

        JToken jTokenVariables = obj["multi-select-enabled"];

        if (jTokenVariables != null)
        {
            result.MultiSelectEnabled = jTokenVariables.Value<string>();
        }

        return result;
    }
}