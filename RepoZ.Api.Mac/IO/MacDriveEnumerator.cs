using RepoZ.Api.IO;
using System;
using System.Linq;

namespace RepoZ.Api.Mac.IO
{
    public class MacDriveEnumerator : IPathProvider
    {
        public string[] GetPaths()
        {
            return System.IO.DriveInfo.GetDrives()
                .Where(d => d.DriveType == System.IO.DriveType.Fixed)
                .Where(p => !p.RootDirectory.FullName.StartsWith("/private", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.RootDirectory.FullName)
                .ToArray();
        }
    }
}