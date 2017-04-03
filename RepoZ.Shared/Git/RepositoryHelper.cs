using System.Diagnostics;
using LibGit2Sharp;

namespace RepoZ.Shared
{
	public class RepositoryReader : IRepositoryReader
	{
		public RepositoryInfo ReadRepository(string path)
		{
			if (string.IsNullOrEmpty(path))
				return RepositoryInfo.Empty;

			string repoPath = Repository.Discover(path);
			if (string.IsNullOrEmpty(repoPath))
				return RepositoryInfo.Empty;

			using (var repo = new Repository(repoPath))
			{
				return new RepositoryInfo()
				{
					Name = new System.IO.DirectoryInfo(repo.Info.WorkingDirectory).Name,
					Path = repo.Info.WorkingDirectory,
					CurrentBranch = repo.Head.FriendlyName
				};
			}
		}

		[DebuggerDisplay("{Name}")]
		public class RepositoryInfo
		{
			public string Name { get; set; }
			public string Path { get; set; }
			public string CurrentBranch { get; set; }

			public static RepositoryInfo Empty => new RepositoryInfo()
			{
				Name = "-",
				CurrentBranch = "-",
				Path = "-"
			};

			public bool WasFound => !string.IsNullOrWhiteSpace(Path) && Path != "-";
		}
	}
}
