using System;
using System.Linq;
using LibGit2Sharp;
using RepoZ.Api.Git;
using System.Collections.Generic;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryWriter : IRepositoryWriter
	{
		private readonly IGitCommander _gitCommander;

		public DefaultRepositoryWriter(IGitCommander gitCommander)
		{
			_gitCommander = gitCommander ?? throw new ArgumentNullException(nameof(gitCommander));
		}

		public bool Checkout(Api.Git.Repository repository, string branchName)
		{
           
            using (var repo = new LibGit2Sharp.Repository(repository.Path))
            {
                string realBranchName = branchName;
                Branch branch;

                // Check if local branch exists
                if (repo.Branches.Any(b => b.FriendlyName == branchName))
                {
                    branch = Commands.Checkout(repo, branchName);
                }
                else
                {
                    // Create local branch to remote branch tip and set its upstream branch to remote
                    var upstreamBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName.EndsWith(branchName));
                    branch = repo.CreateBranch(branchName, upstreamBranch.Tip);     
                    this.SetUpstream(repository, branchName, upstreamBranch.FriendlyName);

                    branch = Commands.Checkout(repo, branchName);
                }


                return branch.FriendlyName == branchName;
            }

		}

		public void Fetch(Api.Git.Repository repository)
		{
			_gitCommander.Command(repository, "fetch");
		}
        public void FetchAll(Api.Git.Repository repository)
        {
            _gitCommander.Command(repository, "fetch", "--all", "--prune");
        }

		public void Pull(Api.Git.Repository repository)
		{
			_gitCommander.Command(repository, "pull");
		}

		public void Push(Api.Git.Repository repository)
		{
			_gitCommander.Command(repository, "push");
		}
        private void SetUpstream(Api.Git.Repository repository, string localBranchName, string upstreamBranchName)
        {
            _gitCommander.Command(repository, "branch", $"--set-upstream-to={upstreamBranchName}", localBranchName);
        }
    }
}
