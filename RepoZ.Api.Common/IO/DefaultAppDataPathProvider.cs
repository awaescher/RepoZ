using RepoZ.Api.IO;
using System;
using System.IO;

namespace RepoZ.Api.Common.IO
{
	public class DefaultAppDataPathProvider : IAppDataPathProvider
	{
		public string GetAppDataPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoZ");
	}
}
