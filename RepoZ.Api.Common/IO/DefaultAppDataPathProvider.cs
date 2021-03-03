using RepoZ.Api.IO;
using System;
using System.IO;

namespace RepoZ.Api.Common.IO
{
	public class DefaultAppDataPathProvider : IAppDataPathProvider
	{
		public virtual string GetAppDataPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoZ");

		public virtual string GetAppResourcesPath() => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
	}
}
