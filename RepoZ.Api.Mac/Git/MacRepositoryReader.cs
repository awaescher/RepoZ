using System;
using RepoZ.Api.Git;

namespace RepoZ.Api.Mac.Git
{
	public class MacRepositoryReader : IRepositoryReader
	{
		public Repository ReadRepository(string path)
		{
			return new Repository()
			{
				Name = System.IO.Path.GetFileName(path),
				CurrentBranch = System.IO.Path.GetFileName(path) + "_branch",
				Path = path,
				AheadBy = 2,
				BehindBy = 1
			};
		}
	}
}
