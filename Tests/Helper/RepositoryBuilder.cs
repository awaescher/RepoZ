using RepoZ.Api.Git;

namespace Tests.Helper
{
    public class RepositoryBuilder
    {
        private readonly Repository _repository;

        public RepositoryBuilder()
        {
            _repository = new Repository();
        }

        public RepositoryBuilder WithName(string name)
        {
            _repository.Name = name;
            return this;
        }

        public RepositoryBuilder WithPath(string path)
        {
            _repository.Path = path;
            return this;
        }

        public RepositoryBuilder WithAheadBy(int ahead)
        {
            _repository.AheadBy = ahead;
            return this;
        }

        public RepositoryBuilder WithBehindBy(int behind)
        {
            _repository.BehindBy = behind;
            return this;
        }

        public RepositoryBuilder WithBranches(params string[] branches)
        {
            _repository.Branches = branches;
            return this;
        }

        public RepositoryBuilder WithCurrentBranch(string currentBranch)
        {
            _repository.CurrentBranch = currentBranch;
            return this;
        }

        public RepositoryBuilder WithDetachedHeadOnCommit(string sha)
        {
            _repository.CurrentBranchIsDetached = true;
            _repository.CurrentBranch = sha;
            return this;
        }

        public RepositoryBuilder WithDetachedHeadOnTag(string tag)
        {
            _repository.CurrentBranchIsDetached = true;
            _repository.CurrentBranchIsOnTag = true;
            _repository.CurrentBranch = tag;
            return this;
        }

        public RepositoryBuilder WithUpstream()
        {
            _repository.CurrentBranchHasUpstream = true;
            return this;
        }

        public RepositoryBuilder WithoutUpstream()
        {
            _repository.CurrentBranchHasUpstream = false;
            return this;
        }

        public RepositoryBuilder WithLocalAdded(int added)
        {
            _repository.LocalAdded = added;
            return this;
        }

        public RepositoryBuilder WithLocalIgnored(int ignored)
        {
            _repository.LocalIgnored = ignored;
            return this;
        }

        public RepositoryBuilder WithLocalMissing(int missing)
        {
            _repository.LocalMissing = missing;
            return this;
        }

        public RepositoryBuilder WithLocalModified(int modified)
        {
            _repository.LocalModified = modified;
            return this;
        }

        public RepositoryBuilder WithLocalRemoved(int removed)
        {
            _repository.LocalRemoved = removed;
            return this;
        }

        public RepositoryBuilder WithLocalStaged(int staged)
        {
            _repository.LocalStaged = staged;
            return this;
        }

        public RepositoryBuilder WithLocalUntracked(int untracked)
        {
            _repository.LocalUntracked = untracked;
            return this;
        }

        public RepositoryBuilder WithStashCount(int stashCount)
        {
            _repository.StashCount = stashCount;
            return this;
        }


        public Repository Build() => _repository;

        public Repository BuildFullFeatured()
        {
            WithUpstream();
            WithAheadBy(1);
            WithBehindBy(2);
            WithBranches("master", "feature-one", "feature-two");
            WithCurrentBranch("master");
            WithLocalAdded(3);
            WithLocalIgnored(4);
            WithLocalMissing(5);
            WithLocalModified(6);
            WithLocalRemoved(7);
            WithLocalStaged(8);
            WithLocalUntracked(9);
            WithStashCount(10);
            WithName("Repo");
            WithPath("C:\\Develop\\Repo\\");

            return Build();
        }
    }
}