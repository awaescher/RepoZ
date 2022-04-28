namespace RepoZ.Api.Common.IO
{
    using RepoZ.Api.IO;
    using System;
    using System.IO;

    public class DefaultAppDataPathProvider : IAppDataPathProvider
    {
        public string GetAppDataPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoZ");
        }

        public string GetAppResourcesPath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }
    }
}