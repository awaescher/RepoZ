using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;
using System.Diagnostics;
using System.Globalization;

namespace RepoZ.Api.Git
{
	[DebuggerDisplay("{Name} @{Path}")]
	public class RepositoryView
	{
		private string _cachedRepositoryStatusCode;
		private string _cachedRepositoryStatus;
		private string _cachedRepositoryStatusWithBranch;

		public RepositoryView(Repository repository)
		{
			if (repository == null)
				throw new ArgumentNullException(nameof(repository));

			Repository = repository;
		}

		public string Name => Repository.Name ?? "";

		public string Path => Repository.Path ?? "";

		public string Location => Repository.Location ?? "";

		public string CurrentBranch => Repository.CurrentBranch ?? "";

		public string AheadBy => Repository.AheadBy?.ToString() ?? "";

		public string BehindBy => Repository.BehindBy?.ToString() ?? "";

		public string[] Branches => Repository.Branches ?? new string[0];

		public string LocalUntracked => Repository.LocalUntracked?.ToString() ?? "";

		public string LocalModified => Repository.LocalModified?.ToString() ?? "";

		public string LocalMissing => Repository.LocalMissing?.ToString() ?? "";

		public string LocalAdded => Repository.LocalAdded?.ToString() ?? "";

		public string LocalStaged => Repository.LocalStaged?.ToString() ?? "";

		public string LocalRemoved => Repository.LocalRemoved?.ToString() ?? "";

		public string LocalIgnored => Repository.LocalIgnored?.ToString() ?? "";

		public bool WasFound => Repository.WasFound;

		public override int GetHashCode() => Repository.GetHashCode();

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

		public override bool Equals(object obj)
		{
			var other = obj as RepositoryView;

			if (other != null)
				return other.Repository.Equals(this.Repository);

			return object.ReferenceEquals(this, obj);
		}

		private void EnsureStatusCache()
		{
			var repositoryStatusCode = Repository.GetStatusCode();

			// compare the status code and not the full status string because the latter one is heavier to calculate
			bool canTakeFromCache = _cachedRepositoryStatusCode == repositoryStatusCode;

			if (!canTakeFromCache)
			{
				var compressor = new StatusCompressor(new StatusCharacterMap());
				_cachedRepositoryStatus = compressor.Compress(Repository);
				_cachedRepositoryStatusWithBranch = compressor.CompressWithBranch(Repository);

				_cachedRepositoryStatusCode = repositoryStatusCode;
			}
		}
	}
}
