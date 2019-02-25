using RepoZ.Api.IO;
using System;

namespace RepoZ.Api.Mac.IO
{
    public class MacDriveEnumerator : IPathProvider
    {
        public string[] GetPaths()
        {
            return new string[] { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) };
        }
    }
}