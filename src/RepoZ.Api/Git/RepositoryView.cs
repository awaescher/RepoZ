namespace RepoZ.Api.Git
{
    using System;
    using System.Diagnostics;
    using System.ComponentModel;

    [DebuggerDisplay("{Name} @{Path}")]
    public class RepositoryView : IRepositoryView, INotifyPropertyChanged
    {
        private string _cachedRepositoryStatusCode;
        private string _cachedRepositoryStatus;
        private string _cachedRepositoryStatusWithBranch;
        private bool _isSynchronizing;

        public event PropertyChangedEventHandler PropertyChanged;

        public RepositoryView(Repository repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            UpdateStampUtc = DateTime.UtcNow;
        }

        public override bool Equals(object obj)
        {
            if (obj is RepositoryView other)
            {
                return other.Repository.Equals(Repository);
            }

            return object.ReferenceEquals(this, obj);
        }

        private void EnsureStatusCache()
        {
            var repositoryStatusCode = Repository.GetStatusCode();

            // compare the status code and not the full status string because the latter one is heavier to calculate
            var canTakeFromCache = _cachedRepositoryStatusCode == repositoryStatusCode;

            if (!canTakeFromCache)
            {
                var compressor = new StatusCompressor(new StatusCharacterMap());
                _cachedRepositoryStatus = compressor.Compress(Repository);
                _cachedRepositoryStatusWithBranch = compressor.CompressWithBranch(Repository);

                _cachedRepositoryStatusCode = repositoryStatusCode;
            }
        }

        public string Name => (Repository.Name ?? string.Empty) + (IsSynchronizing ? SyncAppendix : string.Empty);

        public string Path => Repository.Path ?? string.Empty;

        public string Location => Repository.Location ?? string.Empty;

        public string CurrentBranch => Repository.CurrentBranch ?? string.Empty;

        public string[] ReadAllBranches()
        {
            return Repository.ReadAllBranches() ?? Array.Empty<string>();
        }

        public string AheadBy => Repository.AheadBy?.ToString() ?? string.Empty;

        public string BehindBy => Repository.BehindBy?.ToString() ?? string.Empty;

        public string[] Branches => Repository.Branches ?? Array.Empty<string>();

        public string LocalUntracked => Repository.LocalUntracked?.ToString() ?? string.Empty;

        public string LocalModified => Repository.LocalModified?.ToString() ?? string.Empty;

        public string LocalMissing => Repository.LocalMissing?.ToString() ?? string.Empty;

        public string LocalAdded => Repository.LocalAdded?.ToString() ?? string.Empty;

        public string LocalStaged => Repository.LocalStaged?.ToString() ?? string.Empty;

        public string LocalRemoved => Repository.LocalRemoved?.ToString() ?? string.Empty;

        public string LocalIgnored => Repository.LocalIgnored?.ToString() ?? string.Empty;

        public string[] RemoteUrls => Repository.RemoteUrls ?? Array.Empty<string>();

        public string StashCount => Repository.StashCount?.ToString() ?? string.Empty;

        public bool WasFound => Repository.WasFound;

        public bool HasUnpushedChanges => Repository.HasUnpushedChanges;

        public override int GetHashCode()
        {
            return Repository.GetHashCode();
        }

        public Repository Repository { get; }

        public string Status
        {
            get
            {
                EnsureStatusCache();
                return _cachedRepositoryStatus;
            }
        }

        public string BranchWithStatus
        {
            get
            {
                EnsureStatusCache();
                return _cachedRepositoryStatusWithBranch;
            }
        }

        public bool IsSynchronizing
        {
            get => _isSynchronizing;
            set
            {
                _isSynchronizing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); // Name includes the activity icon
            }
        }

        private string SyncAppendix => "  \u2191\u2193"; // up and down arrows

        public DateTime UpdateStampUtc { get; private set; }

    }
}