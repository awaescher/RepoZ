using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Git
{
	[DebuggerDisplay("{Name}")]
	public class Repository
	{
		public override bool Equals(object obj)
		{
			var other = obj as Repository;
			if (other == null)
				return false;

			return string.Equals(other.Path, this.Path);
		}

		public override int GetHashCode()
		{
			if (string.IsNullOrEmpty(Path))
				return base.GetHashCode();

			return Path.GetHashCode();
		}

		public string Name { get; set; }

		public string Path { get; set; }

		public string CurrentBranch { get; set; }

		public string[] Branches { get; set; }

		public string[] LocalBranches { get; set; }

		public static Repository Empty => new Repository()
		{
			Name = "",
			CurrentBranch = "",
			Path = ""
		};

		public bool CurrentBranchHasUpstream { get; set; }

		public bool WasFound => !string.IsNullOrWhiteSpace(Path);

		public int? AheadBy { get; set; }

		public int? BehindBy { get; set; }

		public int? LocalUntracked { get; set; }

		public int? LocalModified { get; set; }

		public int? LocalMissing { get; set; }

		public int? LocalAdded { get; set; }

		public int? LocalStaged { get; set; }

		public int? LocalRemoved { get; set; }

		public int? LocalIgnored { get; set; }
	}
}
