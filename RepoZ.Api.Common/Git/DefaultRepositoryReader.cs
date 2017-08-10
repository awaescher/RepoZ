using System.Linq;
using LibGit2Sharp;
using RepoZ.Api.Git;
using System.IO;
using System;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryReader : IRepositoryReader
	{
		public Api.Git.Repository ReadRepository(string path)
		{
			if (string.IsNullOrEmpty(path))
				return Api.Git.Repository.Empty;

			string repoPath = LibGit2Sharp.Repository.Discover(path);
			if (string.IsNullOrEmpty(repoPath))
				return Api.Git.Repository.Empty;

			return ReadRepositoryWithRetries(repoPath, 3);

		}

		private Api.Git.Repository ReadRepositoryWithRetries(string repoPath, int maxRetries)
		{
			Api.Git.Repository repository = null;
			int currentTry = 1;

			while (repository == null && currentTry <= maxRetries)
			{
				try
				{
					repository = ReadRepositoryInternal(repoPath);
				}
				catch (LockedFileException)
				{
					if (currentTry >= maxRetries)
						throw;
					else
						System.Threading.Thread.Sleep(500);
				}

				currentTry++;
			}

			return repository;
		}

		private Api.Git.Repository ReadRepositoryInternal(string repoPath)
		{
			try
			{
				using (var repo = new LibGit2Sharp.Repository(repoPath))
				{
					var status = repo.RetrieveStatus();

					var workingDirectory = new DirectoryInfo(repo.Info.WorkingDirectory);

					return new Api.Git.Repository()
					{
						Name = workingDirectory.Name,
						Path = workingDirectory.FullName,
						Location = workingDirectory.Parent.FullName,
						Branches = repo.Branches.Select(b => b.FriendlyName).ToArray(),
						LocalBranches = repo.Branches.Where(b => !b.IsRemote).Select(b => b.FriendlyName).ToArray(),
						CurrentBranch = repo.Head.FriendlyName,
						CurrentBranchHasUpstream = !string.IsNullOrEmpty(repo.Head.UpstreamBranchCanonicalName),
						AheadBy = repo.Head.TrackingDetails?.AheadBy,
						BehindBy = repo.Head.TrackingDetails?.BehindBy,
						LocalUntracked = status?.Untracked.Count(),
						LocalModified = status?.Modified.Count(),
						LocalMissing = status?.Missing.Count(),
						LocalAdded = status?.Added.Count(),
						LocalStaged = status?.Staged.Count(),
						LocalRemoved = status?.Removed.Count(),
						LocalIgnored = status?.Ignored.Count()
					};
				}
			}
			catch (Exception)
			{
				return Api.Git.Repository.Empty;
			}
		}
	}
}

