using System;
using System.Linq;
using LibGit2Sharp;
using RepoZ.Api.Git;

namespace RepoZ.Api.Win.Git
{
	public class WindowsRepositoryWriter : IRepositoryWriter
	{
		public bool Checkout(Api.Git.Repository repository, string branchName)
		{
			using (var repo = new LibGit2Sharp.Repository(repository.Path))
			{
				var branch = LibGit2Sharp.Commands.Checkout(repo, branchName);
				return branch.FriendlyName == branchName;
			}
		}
	}
}
