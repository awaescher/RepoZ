namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

using System.Collections.Generic;

public class ActionsCollection
{
    public List<Variable> Variables { get; set; } = new List<Variable>();

    public List<RepositoryAction> Actions { get; set; } = new List<RepositoryAction>();
}