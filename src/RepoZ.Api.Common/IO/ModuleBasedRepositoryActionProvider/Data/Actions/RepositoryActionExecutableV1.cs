namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;

public class RepositoryActionExecutableV1 : RepositoryAction
{
    public List<string> Executables { get; set; } = new List<string>();

    public string Arguments { get; set; }
}