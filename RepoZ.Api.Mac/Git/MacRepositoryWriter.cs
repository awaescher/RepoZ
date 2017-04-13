using System;
using RepoZ.Api.Git;

namespace RepoZ.Api.Mac
{
	public class MacRepositoryWriter : IRepositoryWriter
	{
		public MacRepositoryWriter()
		{
		}

		public bool Checkout(Repository repository, string branchName)
		{
			// TODO
			return false;
		}
	}
}
