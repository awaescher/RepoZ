namespace RepoZ.Api.Git
{
    public interface IRepositoryView
    {
        string Name { get; }

        string CurrentBranch { get; }

        string Path { get; }

        string[] ReadAllBranches();

        bool HasUnpushedChanges { get; }
    }
}