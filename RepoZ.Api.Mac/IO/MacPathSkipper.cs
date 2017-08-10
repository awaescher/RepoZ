using System;
using System.Collections.Generic;
using System.Linq;
using RepoZ.Api.IO;

namespace RepoZ.Api.Mac.IO
{
	public class MacPathSkipper : IPathSkipper
	{
		private List<string> _exclusions;

		public MacPathSkipper()
		{
			_exclusions = new List<string>()
			{
				@".Trash",
			};
		}

		public bool ShouldSkip(string path)
		{
			return _exclusions.Any(ex => path.IndexOf(ex, StringComparison.OrdinalIgnoreCase) > -1);
		}
	}
}
