namespace RepoZ.Api.IO
{
    public interface ISingleGitRepositoryFinderFactory
    {
        string Name { get; }

        bool IsActive { get; }

        IGitRepositoryFinder Create();
    }
}