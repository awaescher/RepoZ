using CommandLine;

namespace grr
{
	public class RepositoryFilterOptions
	{
		[ValueOption(0)]
		public string RepositoryFilter { get; set; }

		[ValueOption(1)]
		public string FileFilter { get; set; }

		[Option('r', "recursive", DefaultValue = false, HelpText = "Defines whether the file filter should be applied recursively or not.")]
		public bool RecursiveFileFilter { get; set; }

		[Option('e', "elevated", DefaultValue = false, HelpText = "Defines whether the files should be opened in an elevated context or not.")]
		public bool RequestElevation { get; set; }

		public bool HasFileFilter => !string.IsNullOrWhiteSpace(FileFilter);
	}
}
