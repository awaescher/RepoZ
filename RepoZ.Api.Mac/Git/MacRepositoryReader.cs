using System.Diagnostics;
using RepoZ.Api.Git;

namespace RepoZ.Api.Mac.Git
{
	public class MacRepositoryReader : IRepositoryReader
	{
		public Api.Git.Repository ReadRepository(string path)
		{
			var r = new LibGit2Sharp.Repository(path);


			// TODO
			return new Api.Git.Repository()
			{
				Name = "Repo",
				Path = path,
				CurrentBranch = "Repobranch",
				AheadBy = 1,
				BehindBy = 1
			};
		}

	}
}
