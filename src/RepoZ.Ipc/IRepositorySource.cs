namespace RepoZ.Ipc
{
    public interface IRepositorySource
    {
        Repository[] GetMatchingRepositories(string repositoryNamePattern);
    }
}