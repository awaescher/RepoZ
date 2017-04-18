using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Git;
using System.Diagnostics;

namespace RepoZ.UI
{
	[DebuggerDisplay("{Name}")]
	public class RepositoryView
	{
		public RepositoryView(Repository repository)
		{
			if (repository == null)
				throw new ArgumentNullException(nameof(repository));

			Repository = repository;
		}

		public string Name => Repository.Name;

		public string Path => Repository.Path;

		public string CurrentBranch => Repository.CurrentBranch;

		public string AheadBy => Repository.AheadBy?.ToString() ?? "";

		public string BehindBy => Repository.BehindBy?.ToString() ?? "";

		public string LocalUntracked => Repository.LocalUntracked?.ToString() ?? "";

		public string LocalModified => Repository.LocalModified?.ToString() ?? "";

		public string LocalMissing => Repository.LocalMissing?.ToString() ?? "";

		public bool WasFound => Repository.WasFound;

		public override int GetHashCode() => Repository.GetHashCode();

		public Repository Repository { get; private set; }

		public override bool Equals(object obj)
		{
			var other = obj as RepositoryView;

			if (other != null)
				return other.Repository.Equals(this.Repository);

			return object.ReferenceEquals(this, obj);
		}
	}
}
