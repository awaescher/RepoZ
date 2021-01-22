using RepoZ.Api.Common.Git.AutoFetch;

namespace RepoZ.Api.Common.Common
{
	public class AppSettings
	{
		public AutoFetchMode AutoFetchMode { get; set; }

		public bool PruneOnFetch { get; set; }

		public static AppSettings Default => new AppSettings() { AutoFetchMode = AutoFetchMode.Off, PruneOnFetch = false };
	}
}
