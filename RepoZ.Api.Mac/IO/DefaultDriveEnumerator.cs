using RepoZ.Api.IO;
using System;
using System.Linq;

namespace RepoZ.Api.Mac.IO
{
	public class DefaultDriveEnumerator : IPathProvider
	{
		public string[] GetPaths()
		{
			return new string[] { "/Users/andreaswascher/source/" };
		}
	}
}