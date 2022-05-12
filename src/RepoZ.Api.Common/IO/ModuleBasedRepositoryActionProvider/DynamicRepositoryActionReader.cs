namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.DynamicRepositoryActionDeserializer;

public class ActionBrowserV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "browser@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction IActionDeserializer.Deserialize(JToken jToken)
    {
        return Deserialize(jToken);
    }

    public RepositoryActionBrowserV1 Deserialize(JToken jToken)
    {
        return jToken.ToObject<RepositoryActionBrowserV1>();
    }
}

public interface IActionDeserializer
{
    bool CanDeserialize(string type);
    RepositoryAction Deserialize(JToken jToken);
}

public class DynamicRepositoryActionDeserializer
{
    private readonly List<IActionDeserializer> _deserializers = new List<IActionDeserializer>()
        {
            new ActionBrowserV1Deserializer(),
        };

    public Task<RepositoryActionConfiguration2> DeserializeAsync(ReadOnlySpan<char> rawContent)
    {
        var repositoryActionConfiguration2 = new RepositoryActionConfiguration2();

        var value = rawContent.ToString();

        var jsonObject = JObject.Parse(
            value,
            new JsonLoadSettings()
            {
                CommentHandling = CommentHandling.Ignore,
            });

        JToken variablesToken;
        JToken repositoryTagsToken;
        JToken tagsToken;

        JToken redirectToken = jsonObject["redirect"];
        repositoryActionConfiguration2.Redirect = DeserializeRedirect(redirectToken);

        var token = jsonObject["repository-specific-env-files"];
        repositoryActionConfiguration2.RepositorySpecificEnvironmentFiles.AddRange(TryDeserializeEnumerable<File>(token));

        token = jsonObject["repository-specific-config-files"];
        repositoryActionConfiguration2.RepositorySpecificConfigFiles.AddRange(TryDeserializeEnumerable<File>(token));

        variablesToken = jsonObject["variables"];
        repositoryActionConfiguration2.Variables.AddRange(TryDeserializeEnumerable<Variable>(variablesToken));

        repositoryTagsToken = jsonObject["repository-tags"];
        if (repositoryTagsToken != null)
        {
            // check if it is style 2
            tagsToken = repositoryTagsToken.SelectToken("tags");
            if (tagsToken != null)
            {
                variablesToken = repositoryTagsToken.SelectToken("variables");
                repositoryActionConfiguration2.TagsCollection.Variables.AddRange(TryDeserializeEnumerable<Variable>(variablesToken));

                repositoryTagsToken = tagsToken;
            }

            // assume style 1
            IList<JToken> variablesListToken = repositoryTagsToken.Children().ToList();
            foreach (JToken item in variablesListToken)
            {
                RepositoryActionTag v = item.ToObject<RepositoryActionTag>();
                if (v != null)
                {
                    repositoryActionConfiguration2.TagsCollection.Tags.Add(v);
                }
            }
        }

        JToken repositoryActionsToken = jsonObject["repository-actions"];
        if (repositoryActionsToken != null)
        {
            // check if it is style 2
            JToken actions = repositoryActionsToken.SelectToken("actions");
            if (actions != null)
            {
                JToken jTokenVariables = repositoryActionsToken.SelectToken("variables");
                repositoryActionConfiguration2.ActionsCollection.Variables.AddRange(TryDeserializeEnumerable<Variable>(jTokenVariables));

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

                repositoryActionConfiguration2.ActionsCollection.Actions.Add(customAction);
            }
        }

        return Task.FromResult(repositoryActionConfiguration2);
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

        if (redirectToken.Type == JTokenType.String)
        {
            /*
            matches:
{
    "redirect": "filename.txt"
}
            */
            var value = redirectToken.Value<string>();
            return new Redirect()
                {
                    Filename = value,
                    //Enabled = 
                };
        }

/*
matches:
{
    "redirect":
    {
      "filename": "",
      "enabled": ""
    }
}
*/
        return redirectToken.ToObject<Redirect>();
    }

    private RepositoryAction DeserializeAction(string type, JToken obj)
    {
        IActionDeserializer instance = _deserializers.FirstOrDefault(x => x.CanDeserialize(type));
        RepositoryAction result = instance == null
            ? obj.ToObject<RepositoryAction>()
            : instance.Deserialize(obj);

        if (result != null)
        {
            JToken jTokenVariables = obj["multi-select-enabled"];

            if (jTokenVariables != null)
            {
                result.MultiSelectEnabled = jTokenVariables.Value<string>();
            }
        }

        return result;
    }

    public class Variable
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string Enabled { get; set; }
    }

    public class RepositoryActionTag
    {
        public string Tag { get; set; }

        public string When { get; set; }
    }

    public class TagsCollection
    {
        public List<Variable> Variables { get; set; } = new List<Variable>();

        public List<RepositoryActionTag> Tags { get; set; } = new List<RepositoryActionTag>();
    }

    public class ActionsCollection
    {
        public List<Variable> Variables { get; set; } = new List<Variable>();

        public List<RepositoryAction> Actions { get; set; } = new List<RepositoryAction>();
    }

    // [browser@1]
    public class RepositoryActionBrowserV1 : RepositoryAction
    {
        public string Url { get; set; }
    }

    public class RepositoryActionFolderV1: RepositoryAction
    {
        public List<RepositoryAction> Actions { get; set; }
    }

    public class RepositoryAction
    {
        public string Type { get; set; }
    
        public string Name { get; set; }
    
        // public string Command { get; set; }
    
        // public List<string> Executables { get; set; } = new List<string>();
    
        // public string Arguments { get; set; }
    
        // public string Keys { get; set; }
    
        public string Active { get; set; }

        public string MultiSelectEnabled { get; set; }
    
        // public List<RepositoryAction> Subfolder { get; set; } = new List<RepositoryAction>();
    }

    public class RepositoryActionConfiguration2
    {
        public Redirect Redirect { get; set; }

        public List<File> RepositorySpecificEnvironmentFiles { get; set; } = new List<File>();

        public List<File> RepositorySpecificConfigFiles { get; set; } = new List<File>();

        public List<Variable> Variables { get; set; } = new List<Variable>();

        public TagsCollection TagsCollection { get; set; } = new TagsCollection();

        public ActionsCollection ActionsCollection { get; set; } = new ActionsCollection();
    }
}

public class File
{
    public string Filename { get; set; }
    public string When { get; set; }
}

public class Redirect
{
    public string Filename { get; set; }

    public string Enabled { get; set; }
}
