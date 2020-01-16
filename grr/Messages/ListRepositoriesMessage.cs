using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RepoZ.Ipc;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public class ListRepositoriesMessage : IMessage
	{
		private readonly string _repositoryFilter;

		public ListRepositoriesMessage()
			: this(null)
		{
		}

		public ListRepositoriesMessage(RepositoryFilterOptions filter)
		{
			_repositoryFilter = filter?.RepositoryFilter ?? "";
		}

		public void Execute(Repository[] repositories)
		{
			// nothing to do
		}

		public string GetRemoteCommand() => string.IsNullOrEmpty(_repositoryFilter)
			? "list:.*" /* show all with RegEx pattern ".*" */
			: $"list:{RegexFilter.Get(_repositoryFilter)}";

		public bool HasRemoteCommand => true;

		public bool ShouldWriteRepositories(Repository[] repositories) => true;
	}
}
