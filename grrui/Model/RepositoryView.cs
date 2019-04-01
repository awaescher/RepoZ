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

		public string Path => Repository.Path ?? "";
	}
}