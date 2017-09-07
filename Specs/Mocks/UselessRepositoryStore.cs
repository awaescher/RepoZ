using System.Collections.Generic;
using RepoZ.Api.Git;

namespace Specs.Mocks
{
	internal class UselessRepositoryStore : IRepositoryStore
	{
		public IEnumerable<string> Get() => new string[0];

		public void Set(IEnumerable<string> paths)
		{
		}
	}
}