using System.Diagnostics;
using LibGit2Sharp;
using RepoZ.Api.Git;

namespace RepoZ.Win.Git
{
	public class WindowsRepositoryReader : IRepositoryReader
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
	}
}
