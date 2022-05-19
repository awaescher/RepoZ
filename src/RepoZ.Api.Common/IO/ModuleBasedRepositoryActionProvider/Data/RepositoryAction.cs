namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

using System.Collections.Generic;

public class RepositoryAction
{
    public string Type { get; set; }

    public string Name { get; set; }

    public string Active { get; set; }

    public string MultiSelectEnabled { get; set; }

    public List<Variable> Variables { get; set; } = new List<Variable>();
}