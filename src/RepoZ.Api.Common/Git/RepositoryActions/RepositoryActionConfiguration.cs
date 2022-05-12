namespace RepoZ.Api.Common.Git
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class RepositoryActionConfiguration
    {
        [JsonProperty("redirect")]
        public string RedirectFile { get; set; }

        [JsonProperty("repository-actions")]
        public List<RepositoryAction> RepositoryActions { get; set; } = new List<RepositoryAction>();

        [JsonProperty("file-associations")]
        public List<FileAssociation> FileAssociations { get; set; } = new List<FileAssociation>();

        [JsonProperty("repository-tags")]
        public List<RepositoryTag> RepositoryTags { get; set; } = new List<RepositoryTag>();

        [JsonIgnore]
        public LoadState State { get; set; } = LoadState.None;

        [JsonIgnore]
        public string LoadError { get; set; }

        [System.Diagnostics.DebuggerDisplay("{Name}")]
        public class RepositoryAction
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("command")]
            public string Command { get; set; }

            [JsonProperty("executables")]
            public List<string> Executables { get; set; } = new List<string>();

            [JsonProperty("arguments")]
            public string Arguments { get; set; }

            [JsonProperty("keys")]
            public string Keys { get; set; }

            [JsonProperty("active")]
            public string Active { get; set; }

            [JsonProperty("subfolder")]
            public List<RepositoryAction> Subfolder { get; set; } = new List<RepositoryAction>();
        }

        [System.Diagnostics.DebuggerDisplay("{Name}")]
        public class FileAssociation
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("extension")]
            public string Extension { get; set; }

            [JsonProperty("executables")]
            public List<string> Executables { get; set; } = new List<string>();

            [JsonProperty("arguments")]
            public string Arguments { get; set; }

            [JsonProperty("active")]
            public string Active { get; set; }
        }

        [System.Diagnostics.DebuggerDisplay("{Tag}")]
        public class RepositoryTag
        {
            [JsonProperty("tag")]
            public string Tag { get; set; }

            [JsonProperty("selector")]
            public string Select { get; set; }
        }

        public enum LoadState
        {
            Ok,
            None,
            Error,
        }
    }

    // variables
    // include-config
    // repository-tags
    // repository-actions
    //    variables
    //    actions
    //      include config from file
    //      include config from process
    //      file-association
    //      process
    //      git actions?
    //      azure devops?
    //      browser
    //      submenu



    /*

{
    "variables":
    [
            {
                "name": "name",
                "value": '{StringContains({Repository.SafePath}, "DRC")}',
                "enabled": true, // <- optional,default true
            },
            {
                "name": "name",
                "value": '{StringContains({Repository.SafePath}, "DRC")}',
                "enabled": '{StringContains()}', 
            }
    ],

    "include-config" :
    [
        {
            "filename": ??
            "enabled": '{StringContains()}',
        }

    ],

    "repository-tags":
    [
            {
                "tag": "DRC",
                "when": '{StringContains({Repository.SafePath}, "DRC")}'
                "enabled": true/false // <- optional, default true
            }

    // of
        "variables" :
        [
{
                "name": "name",
                "value": '{StringContains({Repository.SafePath}, "DRC")}',
                "enabled": true,
            },
            {
                "name": "name",
                "value": '{StringContains({Repository.SafePath}, "DRC")}',
                "enabled": '{StringContains()}',
            }
        ],
        "tags":
        [
            {
                "tag": "DRC",
                "when": '{StringContains({Repository.SafePath}, "DRC")}'
                "enabled": true/false // <- optional, default true
            }
        ]


      
    ],



}


     */



}