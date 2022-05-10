namespace grrui.Model
{
    using RepoZ.Api.Git;
    using System;

    public class RepositoryView : IRepositoryView
    {
        public RepositoryView(RepoZ.Ipc.Repository repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public override string ToString()
        {
            return DisplayText ?? string.Empty;
        }

        public RepoZ.Ipc.Repository Repository { get; }

        public string DisplayText { get; set; }

        public string Name => Repository?.Name ?? "";

        public string CurrentBranch => Repository?.BranchWithStatus ?? string.Empty;

        public string[] ReadAllBranches()
        {
            return Repository.ReadAllBranches() ?? Array.Empty<string>();
        }

        public string Path => Repository.Path ?? string.Empty;

        public bool HasUnpushedChanges => Repository.HasUnpushedChanges;
    }
}