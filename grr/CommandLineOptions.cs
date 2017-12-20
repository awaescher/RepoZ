using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace grr
{
	class CommandLineOptions
	{
		public const string ListCommand = "list";
		public const string ChangeDirectoryCommand = "cd";
		public const string OpenDirectoryCommand = "open";

		public const string HelpCommand = "help";
		public const char HelpCommandChar = '?';

		[VerbOption(ListCommand, HelpText = "Lists the repositories found by RepoZ including their current branch and the corresponding status.")]
		public FilterOptions ListOptions { get; set; }

		[VerbOption(ChangeDirectoryCommand, HelpText = "Navigates to the directory of a given repository.")]
		public FilterOptions ChangeDirectoryOptions { get; set; }

		[VerbOption(OpenDirectoryCommand, HelpText = "Opens the directory of a given repository with the default shell.")]
		public FilterOptions OpenDirectoryOptions { get; set; }

		[Option(HelpCommandChar, HelpCommand, HelpText = "Shows this help page")]
		public bool Help { get; set; }

		public static bool IsKnownArgument(string arg)
		{
			var args = new string[] { ListCommand, ChangeDirectoryCommand, OpenDirectoryCommand, HelpCommand, HelpCommandChar.ToString() };
			arg = arg.TrimStart('-').TrimStart('/');

			return args.Contains(arg, StringComparer.OrdinalIgnoreCase);
		}

		[HelpOption]
		public static string GetUsage()
		{
			var help = new HelpText
			{
				Heading = HeadingInfo.Default,
				Copyright = CopyrightInfo.Default,
				AdditionalNewLineAfterOption = true,
				AddDashesToOption = false
			};

			help.AddOptions(new CommandLineOptions());

			help.AddPostOptionsLine("Usage:");
			help.AddPostOptionsLine("  grr \t\t\tLists all repositories found in RepoZ");
			help.AddPostOptionsLine("  grr list SomeRepo \tShows the branch and status of a given repository");
			help.AddPostOptionsLine("  grr cd SomeRepo \tNavigates to the main directory of a given repository");
			help.AddPostOptionsLine("  grr open SomeRepo \tOpens the main directory of a given repository with the default shell");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("Use RegEx patterns for advanced filtering:");
			help.AddPostOptionsLine("  grr list .*_.* \tLists all repositories containing a \"_\"");
			help.AddPostOptionsLine("  grr list \".*[X|Z]\" \tLists all repositories ending with \"X\" or \"Y\"");
			help.AddPostOptionsLine("  grr cd Re.* \t\tNavigates to the first repository starting with \"Re\"");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("Advanced: grr defines indexes for found repositories.");
			help.AddPostOptionsLine("          They can be used as shortcut for the next execution:");
			help.AddPostOptionsLine("  grr list :3 \tShows the branch and status of the repository on index 3");
			help.AddPostOptionsLine("  grr cd :21 \tNavigates to the repository on index 21");
			help.AddPostOptionsLine("  grr cd - \tNavigates back to the last path grr was called from");
			help.AddPostOptionsLine("  ");
			help.AddPostOptionsLine("Noteworthy:");
			help.AddPostOptionsLine("  The parameter \"list\" can be omitted, \"grr .*_.*\" has the same effect");
			help.AddPostOptionsLine("  RepoZ has to be running on this system to use grr.");
			help.AddPostOptionsLine("");

			return help.ToString();
		}

		internal class FilterOptions
		{
			[ValueOption(0)]
			public string RepositoryFilter { get; set; }

			[ValueOption(1)]
			public string FileFilter { get; set; }

		}
	}
}
