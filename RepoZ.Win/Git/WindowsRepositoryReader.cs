using System.Diagnostics;
using LibGit2Sharp;
using RepoZ.Api.Git;

namespace RepoZ.Win.Git
{
	public class WindowsRepositoryReader : IRepositoryReader
	{
		public Api.Git.Repository ReadRepository(string path)
		{
			if (string.IsNullOrEmpty(path))
				return Api.Git.Repository.Empty;

			string repoPath = LibGit2Sharp.Repository.Discover(path);
			if (string.IsNullOrEmpty(repoPath))
				return Api.Git.Repository.Empty;

			using (var repo = new LibGit2Sharp.Repository(repoPath))
			{
				return new Api.Git.Repository()
				{
					Name = new System.IO.DirectoryInfo(repo.Info.WorkingDirectory).Name,
					Path = repo.Info.WorkingDirectory,
					CurrentBranch = repo.Head.FriendlyName
				};
			}
		}
	}
}
