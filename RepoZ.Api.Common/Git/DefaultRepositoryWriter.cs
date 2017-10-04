using System;
using System.Linq;
using LibGit2Sharp;
using RepoZ.Api.Git;
using System.Collections.Generic;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryWriter : IRepositoryWriter
	{
		private IGitHelpers _helpers;

		public DefaultRepositoryWriter(IGitHelpers helpers)
		{
			_helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
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
			_helpers.Command(repository, "fetch");
		}

		public void Pull(Api.Git.Repository repository)
		{
			_helpers.Command(repository, "pull");
		}

		public void Push(Api.Git.Repository repository)
		{
			_helpers.Command(repository, "push");
		}
	}
}
