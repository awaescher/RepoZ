using RepoZ.Api.Git;
using RepoZ.Ipc;
using System;

namespace grrui.Model
{
    public class RepositoryView : IRepositoryView
    {
        public RepositoryView(RepoZ.Ipc.Repository repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public override string ToString() => DisplayText ?? "";

        public RepoZ.Ipc.Repository Repository { get; }

        public string DisplayText { get; set; }

        public string Name => Repository?.Name ?? "";

        public string CurrentBranch => Repository?.BranchWithStatus ?? "";

        public string[] ReadAllBranches() => Repository.AllBranches ?? new string[0];

        public string Path => Repository.Path ?? "";

        public bool HasUnpushedChanges => Repository.HasUnpushedChanges;
    }
}