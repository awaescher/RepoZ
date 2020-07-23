using RepoZ.Api.Git;
using RepoZ.Ipc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace grrui.Model
{
	public class RepositoriesView
	{
		private const int MAX_REPO_NAME_LENGTH = 35;
		private static StatusCharacterMap _map;
		private RepositoryView[] _repositoryViews;

		public RepositoriesView(IEnumerable<RepoZ.Ipc.Repository> repositories)
		{
			var repositoryCount = repositories.Count();
			_repositoryViews = new RepositoryView[repositoryCount];

			_map = new StatusCharacterMap();

			var maxRepoNameLength = Math.Min(MAX_REPO_NAME_LENGTH, repositories.Max(r => r.Name?.Length ?? 0));
			var maxIndexStringLength = repositoryCount.ToString().Length;
			var writeIndex = repositoryCount > 1;

			for (int i = 0; i < repositoryCount; i++)
			{
				var userIndex = i + 1; // the index visible to the user is 1-based, not 0-based;
				var repository = repositories.ElementAt(i);

				string repoName = (repository.Name.Length > MAX_REPO_NAME_LENGTH)
					? repository.Name.Substring(0, MAX_REPO_NAME_LENGTH) + _map.EllipsesSign
					: repository.Name;

				var index = "";
				if (writeIndex)
					index = $"[{userIndex.ToString().PadLeft(maxIndexStringLength)}]  ";

				var name = repoName.PadRight(maxRepoNameLength + 3);
				var branch = repository.BranchWithStatus;

				var displayText = index + name + branch;

				_repositoryViews[i] = new RepositoryView(repository) { DisplayText = displayText };
			}
		}

		public RepositoryView[] Repositories => _repositoryViews
			.Where(r => r.MatchesFilter(Filter ?? ""))
			.ToArray();

		public string Filter { get; set; }
	}
}