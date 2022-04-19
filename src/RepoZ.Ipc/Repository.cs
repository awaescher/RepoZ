namespace RepoZ.Ipc
{
    using System;

    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class Repository
    {
        public static Repository FromString(string value)
        {
            var parts = value?.Split(new string[] { "::" }, System.StringSplitOptions.None);
            var partsCount = parts?.Length ?? 0;

            var validFormat = partsCount == 3 || partsCount == 4;
            if (!validFormat)
            {
                return null;
            }

            return new Repository()
                {
                    Name = parts[0],
                    BranchWithStatus = parts[1],
                    Path = parts[2],
                    HasUnpushedChanges = parts.Length > 3 && parts[3] == "1",
                };
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(BranchWithStatus) || string.IsNullOrEmpty(Path))
            {
                return "";
            }

            return $"{Name}::{BranchWithStatus}::{Path}::{(HasUnpushedChanges ? "1" : "0")}";
        }

        public string Name { get; set; }

        public string BranchWithStatus { get; set; }

        public string Path { get; set; }

        public string[] ReadAllBranches()
        {
            return Array.Empty<string>();
        }

        public bool HasUnpushedChanges { get; set; }

        public string SafePath => Path?.Replace(@"\", "/") ?? "";
    }
}