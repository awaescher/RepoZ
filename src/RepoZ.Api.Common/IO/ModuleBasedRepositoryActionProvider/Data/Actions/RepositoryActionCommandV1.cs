namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class RepositoryActionCommandV1 : RepositoryAction
{
    public string Command { get; set; }

    public string Arguments { get; set; }
}