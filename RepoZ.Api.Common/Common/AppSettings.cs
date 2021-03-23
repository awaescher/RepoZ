using System.Collections.Generic;
using RepoZ.Api.Common.Git.AutoFetch;

namespace RepoZ.Api.Common.Common
{
	public class AppSettings
	{
		public AutoFetchMode AutoFetchMode { get; set; }

		public bool PruneOnFetch { get; set; }

		public List<ApplicationPath> ExePaths { get; set; }

		public static AppSettings Default => new AppSettings() { AutoFetchMode = AutoFetchMode.Off, PruneOnFetch = false, ExePaths = null };
	}

	public class ApplicationPath
	{
		public string ApplicationName { get; set; }
		public string ApplicationFullPath { get; set; }

	}
}
