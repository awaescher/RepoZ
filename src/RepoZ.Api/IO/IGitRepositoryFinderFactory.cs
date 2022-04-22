namespace RepoZ.Api.IO
{
    public interface IGitRepositoryFinderFactory
    {
        IGitRepositoryFinder Create();
    }
}