namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Git;

public class DynamicRepositoryActionDeserializer
{
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

        // check if there is a variables
        if (jsonObject.ContainsKey("variables"))
        {
            JToken jToken = jsonObject["variables"];
            IList<JToken> variables = jToken.Children().ToList();
            foreach (JToken variable in variables)
            {
                Variable v = variable.ToObject<Variable>();
                if (v != null)
                {
                    repositoryActionConfiguration2.Variables.Add(v);
                }
            }
        }

        if (jsonObject.ContainsKey("repository-tags"))
        {
            JToken jToken = jsonObject["repository-tags"];

            if (jToken != null)
            {
                // check if it is style 2
                JToken tags = jToken.SelectToken("tags");
                if (tags != null)
                {
                    // also check for variables
                    JToken jTokenVariables = jToken.SelectToken("variables");
                    if (jTokenVariables != null)
                    {

                        IList<JToken> variables2 = jTokenVariables.Children().ToList();
                        foreach (JToken variable in variables2)
                        {
                            Variable v = variable.ToObject<Variable>();
                            if (v != null)
                            {
                                repositoryActionConfiguration2.TagCollection.Variables.Add(v);
                            }
                        }
                    }

                    jToken = tags;
                }

                // assume style 1
                IList<JToken> variables = jToken.Children().ToList();
                foreach (JToken variable in variables)
                {
                    RepositoryActionTag v = variable.ToObject<RepositoryActionTag>();
                    if (v != null)
                    {
                        repositoryActionConfiguration2.TagCollection.Tags.Add(v);
                    }
                }
            }
        }

        return Task.FromResult(repositoryActionConfiguration2);

        // repositoryActionConfiguration2.Tags = new List<RepositoryActionTag>()
        //     {
        //         new RepositoryActionTag()
        //             {
        //                 Tag = "work",
        //                 When = "{StringContains({Repository.SafePath}, \"work\")}",
        //             },
        //         new RepositoryActionTag()
        //             {
        //                 Tag = "private",
        //                 When = "{StringContains({Repository.SafePath}, \"Private\")}",
        //             },
        //         new RepositoryActionTag()
        //             {
        //                 Tag = "always-tag",
        //             },
        //     };
        // return Task.FromResult(repositoryActionConfiguration2);
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

    public class RepositoryActionTags
    {
        public List<Variable> Variables { get; set; } = new List<Variable>();

        public List<RepositoryActionTag> Tags { get; set; } = new List<RepositoryActionTag>();
    }

    public class RepositoryActionConfiguration2
    {
        public List<Variable> Variables { get; set; } = new List<Variable>();

        // public List<RepositoryActionTag> Tags { get; set; } = new List<RepositoryActionTag>();
        public RepositoryActionTags TagCollection { get; set; } = new RepositoryActionTags();

        // [JsonProperty("repository-actions")]
        // public List<RepositoryAction> RepositoryActions { get; set; } = new List<RepositoryAction>();
        //
        // [JsonProperty("repository-tags")]
        // public List<RepositoryTag> RepositoryTags { get; set; } = new List<RepositoryTag>();
        //
        // [System.Diagnostics.DebuggerDisplay("{Name}")]
        // public class RepositoryAction
        // {
        //     [JsonProperty("type")]
        //     public string Type { get; set; }
        //
        //     [JsonProperty("name")]
        //     public string Name { get; set; }
        //
        //     [JsonProperty("command")]
        //     public string Command { get; set; }
        //
        //     [JsonProperty("executables")]
        //     public List<string> Executables { get; set; } = new List<string>();
        //
        //     [JsonProperty("arguments")]
        //     public string Arguments { get; set; }
        //
        //     [JsonProperty("keys")]
        //     public string Keys { get; set; }
        //
        //     [JsonProperty("active")]
        //     public string Active { get; set; }
        //
        //     [JsonProperty("subfolder")]
        //     public List<RepositoryAction> Subfolder { get; set; } = new List<RepositoryAction>();
        // }
        //
        //
        //
        // [System.Diagnostics.DebuggerDisplay("{Tag}")]
        // public class RepositoryTag
        // {
        //     [JsonProperty("tag")]
        //     public string Tag { get; set; }
        //
        //     [JsonProperty("selector")]
        //     public string Select { get; set; }
        // }
        //
        // public enum LoadState
        // {
        //     Ok,
        //     None,
        //     Error,
        // }
    }
}

// }