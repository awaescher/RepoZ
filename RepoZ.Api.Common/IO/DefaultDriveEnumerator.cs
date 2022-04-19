namespace RepoZ.Api.Common.IO
{
    using RepoZ.Api.IO;
    using System.Linq;

    public class DefaultDriveEnumerator : IPathProvider
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