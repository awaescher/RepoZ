using RepoZ.Api.IO;
using System;
using System.Linq;

namespace RepoZ.Api.Win.IO
{
	public class WindowsDriveEnumerator : IPathProvider
	{
		public string[] GetPaths()
		{
			return System.IO.DriveInfo.GetDrives()
				.Where(d => d.DriveType == System.IO.DriveType.Fixed)
				.Select(d => d.RootDirectory.FullName)
				.ToArray();
		}
	}
}