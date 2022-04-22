namespace RepoZ.Api.Git
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{Name} @{Path}")]
    public class Repository
    {
        public override bool Equals(object obj)
        {
            if (!(obj is Repository other))
            {
                return false;
            }

            if (string.IsNullOrEmpty(other.Path))
            {
                return string.IsNullOrEmpty(Path);
            }

            return string.Equals(Normalize(other.Path), Normalize(Path), StringComparison.OrdinalIgnoreCase);
        }

        private string Normalize(string path)
        {
            // yeah not that beautiful but we have to add a blackslash
            // or slash (depending on the OS) and on Mono, I dont have Path.PathSeparator.
            // so we add a random char with Path.Combine() and remove it again
            path = System.IO.Path.Combine(path, "_");
            path = path.Substring(0, path.Length - 1);

            return System.IO.Path.GetDirectoryName(path);
        }

        public override int GetHashCode()
        {
            return (Path ?? string.Empty).GetHashCode();
        }

        public string Name { get; set; }

        public string Path { get; set; }

        public string Location { get; set; }

        public string CurrentBranch { get; set; }

        public string[] Branches { get; set; }

        public string[] LocalBranches { get; set; }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Func<string[]> AllBranchesReader { get; set; }

        public string[] ReadAllBranches()
        {
            return AllBranchesReader?.Invoke() ?? Array.Empty<string>();
        }

        public static Repository Empty => new Repository()
            {
                Name = string.Empty,
                CurrentBranch = string.Empty,
                Path = string.Empty,
            };

        public bool CurrentBranchHasUpstream { get; set; }

        public bool CurrentBranchIsDetached { get; set; }

        public bool CurrentBranchIsOnTag { get; set; }

        public bool WasFound => !string.IsNullOrWhiteSpace(Path);

        public bool HasUnpushedChanges => (AheadBy ?? 0) > 0 ||
                                          (LocalUntracked ?? 0) > 0 ||
                                          (LocalModified ?? 0) > 0 ||
                                          (LocalMissing ?? 0) > 0 ||
                                          (LocalAdded ?? 0) > 0 ||
                                          (LocalStaged ?? 0) > 0 ||
                                          (LocalRemoved ?? 0) > 0 ||
                                          (StashCount ?? 0) > 0;

        public int? AheadBy { get; set; }

        public int? BehindBy { get; set; }

        public int? LocalUntracked { get; set; }

        public int? LocalModified { get; set; }

        public int? LocalMissing { get; set; }

        public int? LocalAdded { get; set; }

        public int? LocalStaged { get; set; }

        public int? LocalRemoved { get; set; }

        public int? LocalIgnored { get; set; }

        public int? StashCount { get; set; }

        public string[] RemoteUrls { get; set; }

        public string SafePath
        {
            // use '/' for linux systems and bash command line (will work on cmd and powershell as well)
            get
            {
                var safePath = Path?.Replace(@"\", "/") ?? string.Empty;
                if (safePath.EndsWith("/"))
                {
                    safePath = safePath.Substring(0, safePath.Length - 1);
                }

                return safePath;
            }
        }

        public string GetStatusCode()
        {
            return string.Join("-", new object[]
                {
                    CurrentBranch ?? string.Empty,
                    AheadBy ?? 0,
                    BehindBy ?? 0,
                    LocalUntracked ?? 0,
                    LocalModified ?? 0,
                    LocalMissing ?? 0,
                    LocalAdded ?? 0,
                    LocalStaged ?? 0,
                    LocalRemoved ?? 0,
                    LocalIgnored ?? 0,
                    StashCount ?? 0
                });
        }
    }
}