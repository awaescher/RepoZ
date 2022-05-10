namespace grrui.Model
{
    using RepoZ.Api.Git;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Repository = RepoZ.Ipc.Repository;

    public class RepositoriesView
    {
        private const int MAX_REPO_NAME_LENGTH = 35;
        private readonly RepositoryView[] _repositoryViews;

        public RepositoriesView(IEnumerable<RepoZ.Ipc.Repository> repositories)
        {
            Repository[] repos = repositories as Repository[] ?? repositories.ToArray();

            var repositoryCount = repos.Length;
            _repositoryViews = new RepositoryView[repositoryCount];

            var map = new StatusCharacterMap();

            var maxRepoNameLength = Math.Min(MAX_REPO_NAME_LENGTH, repos.Max(r => r.Name?.Length ?? 0));
            var maxIndexStringLength = repositoryCount.ToString().Length;
            var writeIndex = repositoryCount > 1;

            for (var i = 0; i < repositoryCount; i++)
            {
                var userIndex = i + 1; // the index visible to the user is 1-based, not 0-based;
                Repository repository = repos.ElementAt(i);

                var repoName = (repository.Name.Length > MAX_REPO_NAME_LENGTH)
                    ? repository.Name.Substring(0, MAX_REPO_NAME_LENGTH) + map.EllipsesSign
                    : repository.Name;

                var index = "";
                if (writeIndex)
                {
                    index = $"[{userIndex.ToString().PadLeft(maxIndexStringLength)}]  ";
                }

                var name = repoName.PadRight(maxRepoNameLength + 3);
                var branch = repository.BranchWithStatus;

                var displayText = index + name + branch;

                _repositoryViews[i] = new RepositoryView(repository) { DisplayText = displayText };
            }
        }

        public RepositoryView[] Repositories => _repositoryViews
                                                .Where(r => r.MatchesFilter(Filter ?? string.Empty))
                                                .ToArray();

        public string Filter { get; set; }
    }
}