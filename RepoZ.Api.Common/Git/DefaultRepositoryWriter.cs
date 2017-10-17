using System;
using System.Linq;
using LibGit2Sharp;
using RepoZ.Api.Git;
using System.Collections.Generic;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryWriter : IRepositoryWriter
	{
		private IGitCommander _gitCommander;

		public DefaultRepositoryWriter(IGitCommander gitCommander)
		{
			_gitCommander = gitCommander ?? throw new ArgumentNullException(nameof(gitCommander));
		}

		public bool Checkout(Api.Git.Repository repository, string branchName)
		{
			using (var repo = new LibGit2Sharp.Repository(repository.Path))
			{
				var branch = Commands.Checkout(repo, branchName);
				return branch.FriendlyName == branchName;
			}
		}

		public void Fetch(Api.Git.Repository repository)
		{
			_gitCommander.Command(repository, "fetch");
		}

		public void Pull(Api.Git.Repository repository)
		{
			_gitCommander.Command(repository, "pull");
		}

		public void Push(Api.Git.Repository repository)
		{
			_gitCommander.Command(repository, "push");
		}
	}
}
