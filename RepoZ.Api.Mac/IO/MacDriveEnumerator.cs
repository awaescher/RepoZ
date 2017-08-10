using RepoZ.Api.IO;
using System;
using System.Linq;

namespace RepoZ.Api.Mac.IO
{
	public class MacDriveEnumerator : IPathProvider
	{
		public string[] GetPaths()
		{
			// TODO
			return new string[] { "/Users/andreaswascher/source/" };
		}
	}
}