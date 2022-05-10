namespace grr.Messages
{
    using RepoZ.Ipc;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;

    [System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
    public class ListRepositoryFilesMessage : FileMessage
    {
        public ListRepositoryFilesMessage(RepositoryFilterOptions filter, IFileSystem fileSystem)
            : base(filter, fileSystem) { }

        protected override void ExecuteFound(string[] files)
        {
            foreach (var file in files)
            {
                System.Console.WriteLine(file);
            }
        }

        protected override IEnumerable<string> FindItems(string directory, RepositoryFilterOptions filter)
        {
            SearchOption searchOption = Filter.RecursiveFileFilter
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            // todo Fix IFileSystem
            return /*FileSystem.*/Directory.GetFileSystemEntries(directory, filter.FileFilter, searchOption)
                                      .OrderBy(i => i);
        }

        public override bool ShouldWriteRepositories(Repository[] repositories)
        {
            return false;
        }
    }
}