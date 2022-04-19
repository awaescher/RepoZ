namespace grr.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    [System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
    public abstract class FileMessage : DirectoryMessage
    {
        public FileMessage(RepositoryFilterOptions filter)
            : base(filter)
        {
        }

        protected override void ExecuteExistingDirectory(string directory)
        {
            string[] items = null;

            try
            {
                items = FindItems(directory, Filter).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:\n" + ex.ToString());
                return;
            }

            if (items.Length == 0)
            {
                System.Console.WriteLine($"No files found.\n  Directory:\t{directory}\n  Filter:\t{Filter.FileFilter}");
                return;
            }

            ExecuteFound(items);
        }

        protected virtual IEnumerable<string> FindItems(string directory, RepositoryFilterOptions filter)
        {
            SearchOption searchOption = Filter.RecursiveFileFilter
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            return Directory.GetFiles(directory, filter.FileFilter, searchOption)
                            .OrderBy(i => i);
        }

        protected abstract void ExecuteFound(string[] files);
    }
}
