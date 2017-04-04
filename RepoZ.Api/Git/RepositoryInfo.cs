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
		public string Name { get; set; }
		public string Path { get; set; }
		public string CurrentBranch { get; set; }

		public static Repository Empty => new Repository()
		{
			Name = "",
			CurrentBranch = "",
			Path = ""
		};

		public bool WasFound => !string.IsNullOrWhiteSpace(Path);

		public override bool Equals(object obj)
		{
			var other = obj as Repository;
			if (other == null)
				return false;

			return string.Equals(other.Path, this.Path);
		}
	}
}
