namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;

public class RepositoryActionFolderV1 : RepositoryAction
{
    public List<RepositoryAction> Items { get; set; } = new List<RepositoryAction>();

    public string IsDeferred { get; set; }
}