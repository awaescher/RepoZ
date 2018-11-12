using CommandLine;

namespace grr
{
	public class RepositoryFilterOptions
	{
		[Value(0)]
		public string RepositoryFilter { get; set; }

		[Value(1)]
		public string FileFilter { get; set; }

		[Option('r', "recursive", Default = false, HelpText = "Defines whether the file filter should be applied recursively or not.")]
		public bool RecursiveFileFilter { get; set; }

		[Option('e', "elevated", Default = false, HelpText = "Defines whether the files should be opened in an elevated context or not.")]
		public bool RequestElevation { get; set; }

		public bool HasFileFilter => !string.IsNullOrWhiteSpace(FileFilter);
	}
}
