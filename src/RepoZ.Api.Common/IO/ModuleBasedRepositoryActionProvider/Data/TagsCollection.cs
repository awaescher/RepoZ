namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

using System.Collections.Generic;

public class TagsCollection
{
    public List<Variable> Variables { get; set; } = new List<Variable>();

    public List<RepositoryActionTag> Tags { get; set; } = new List<RepositoryActionTag>();
}