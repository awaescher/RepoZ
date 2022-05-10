namespace grr.Messages
{
    using System;
    using System.IO.Abstractions;
    using RepoZ.Ipc;

    [System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
    public abstract class DirectoryMessage : IMessage
    {
        private readonly bool _argumentIsExistingDirectory;
        private protected readonly IFileSystem FileSystem;

        public DirectoryMessage(RepositoryFilterOptions filter, IFileSystem fileSystem)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _argumentIsExistingDirectory = FileSystem.Directory.Exists(Filter.RepositoryFilter);
        }

        public void Execute(Repository[] repositories)
        {
            if (_argumentIsExistingDirectory)
            {
                ExecuteExistingDirectoryWithSafetyCheck(Filter.RepositoryFilter);
            }
            else
            {
                ExecuteRepositoryQuery(repositories);
            }
        }

        private void ExecuteExistingDirectoryWithSafetyCheck(string directory)
        {
            // use '/' for linux systems and bash command line (will work on cmd and powershell as well)
            directory = directory.Replace(@"\", "/");
            ExecuteExistingDirectory(directory);
        }

        protected abstract void ExecuteExistingDirectory(string directory);

        protected virtual void ExecuteRepositoryQuery(Repository[] repositories)
        {
            if (repositories == null || repositories.Length <= 0)
            {
                return;
            }

            foreach (Repository repository in repositories)
            {
                var directory = repository.SafePath;

                if (string.IsNullOrWhiteSpace(directory))
                {
                    System.Console.WriteLine("Repository path is empty. Aborting.");
                    return;
                }

                if (FileSystem.Directory.Exists(directory))
                {
                    ExecuteExistingDirectoryWithSafetyCheck(directory);
                }
                else
                {
                    System.Console.WriteLine("Repository path does not exist:\n" + directory);
                }
            }
        }

        public virtual string GetRemoteCommand()
        {
            if (!HasRemoteCommand)
            {
                return null;
            }

            return string.IsNullOrEmpty(Filter?.RepositoryFilter)
                ? null /* makes no sense */
                : $"list:{RegexFilter.Get(Filter.RepositoryFilter)}";
        }

        public virtual bool HasRemoteCommand
        {
            get
            {
                if (_argumentIsExistingDirectory)
                {
                    return false;
                }

                return !string.IsNullOrEmpty(Filter?.RepositoryFilter);
            }
        }

        public abstract bool ShouldWriteRepositories(Repository[] repositories);

        public RepositoryFilterOptions Filter { get; }
    }
}