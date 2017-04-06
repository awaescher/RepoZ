using System;
using System.Linq;
using RepoZ.Api.IO;

namespace RepoZ.Api.Mac.IO
{
	public class MacDriveEnumerator : IPathProvider
	{
		public MacDriveEnumerator()
		{
		}

		public string[] GetPaths()
		{
			return System.IO.DriveInfo.GetDrives()
				         .Where(d => d.DriveType == System.IO.DriveType.Fixed)
						 .Select(d => d.RootDirectory.FullName)
						 .ToArray();
		}
	}
}
