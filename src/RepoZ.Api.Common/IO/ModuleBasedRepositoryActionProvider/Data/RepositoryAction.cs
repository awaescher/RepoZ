namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

using System.Collections.Generic;

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

    public List<Variable> Variables { get; set; } = new List<Variable>();
}