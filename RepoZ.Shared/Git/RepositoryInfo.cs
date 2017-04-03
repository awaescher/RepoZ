using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Shared.Git
{
	[DebuggerDisplay("{Name}")]
	public class RepositoryInfo
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string CurrentBranch { get; set; }

		public static RepositoryInfo Empty => new RepositoryInfo()
		{
			Name = "-",
			CurrentBranch = "-",
			Path = "-"
		};

		public bool WasFound => !string.IsNullOrWhiteSpace(Path) && Path != "-";
	}
}
