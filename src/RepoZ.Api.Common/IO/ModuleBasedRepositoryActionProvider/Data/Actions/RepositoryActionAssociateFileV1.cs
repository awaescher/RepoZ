namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class RepositoryActionAssociateFileV1 : RepositoryAction
{
    public string Extension { get; set; }

    public string Command { get; set; }

    public string Arguments { get; set; }
}