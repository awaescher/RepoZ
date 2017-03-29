using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace branch.Shared
{
	public class RepositoryHelper
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
		}
	}
}
