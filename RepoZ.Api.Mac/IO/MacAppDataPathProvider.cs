using System;
using System.IO;
using RepoZ.Api.Common.IO;

namespace RepoZ.Api.Mac.IO
{
	public class MacAppDataPathProvider : DefaultAppDataPathProvider
	{
		public override string GetAppResourcesPath() => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "..", "Resources");
	}
}
