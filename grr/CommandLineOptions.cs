using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace grr
{
	partial class CommandLineOptions
	{
		public const string ListCommand = "list";
		public const string ChangeDirectoryCommand = "cd";
		public const string OpenDirectoryCommand = "open";

		public const string HelpCommand = "help";
		public const char HelpCommandChar = '?';

		[VerbOption(ListCommand, HelpText = "(Default) Lists the repositories found by RepoZ including their current branch and the corresponding status. Can be omitted like shown in the examples below.")]
		public RepositoryFilterOptions ListOptions { get; set; }

		[VerbOption(ChangeDirectoryCommand, HelpText = "Causes the command line interface to navigate to the directory of a given repository.")]
		public RepositoryFilterOptions ChangeDirectoryOptions { get; set; }

		[VerbOption(OpenDirectoryCommand, HelpText = "Opens the directory or a file of a given repository with the operating system's default application.")]
		public RepositoryFilterOptions OpenDirectoryOptions { get; set; }

		[Option(HelpCommandChar, HelpCommand, HelpText = "Shows this help page")]
		public bool Help { get; set; }

		public static string[] GetKnownCommands() => new string[] { ListCommand, ChangeDirectoryCommand, OpenDirectoryCommand, HelpCommand, HelpCommandChar.ToString() };

		public static bool IsKnownArgument(string arg)
		{
			arg = arg.TrimStart('-').TrimStart('/');
			return GetKnownCommands().Contains(arg, StringComparer.OrdinalIgnoreCase);
		}

		[HelpOption]
		public static string GetUsage()
		{
			var help = new HelpText
			{
				Heading = HeadingInfo.Default,
				Copyright = CopyrightInfo.Default,
				AdditionalNewLineAfterOption = true,
				AddDashesToOption = false,
				MaximumDisplayWidth = 100
			};

			string knownCommandsPiped = string.Join("|", GetKnownCommands());
			help.AddPreOptionsLine(" ");
			help.AddPreOptionsLine(" ");
			help.AddPreOptionsLine("USAGE:");
			help.AddPreOptionsLine($"  grr [{knownCommandsPiped}] [repository name or filter] [file name or filter] [file options]");
			help.AddPreOptionsLine(" ");
			help.AddPreOptionsLine(" ");
			help.AddPreOptionsLine(" ");

			help.AddPreOptionsLine("COMMANDS:");
			help.AddOptions(new CommandLineOptions());

			help.AddPostOptionsLine(" ");
			help.AddPostOptionsLine("FILTERS:");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("  Repository name or filter:");
			help.AddPostOptionsLine("    The name of a repository or a RegEx filter expression to find matching repositories.");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("  File name or filter:");
			help.AddPostOptionsLine("    The name of a file or a filter pattern to find matching files of a given repository.");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("OPTIONS:");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("  --recursive or -r:");
			help.AddPostOptionsLine("    Enables recursive search in subdirectories of a given Git repository.");
			help.AddPostOptionsLine("    Compatible with commands: \"list\" and \"open\"");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("  --elevated or -e:");
			help.AddPostOptionsLine("    Invokes the UAC dialog to request elevated priviledges for the process to open.");
			help.AddPostOptionsLine("    Compatible with commands: \"open\"");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("");


			help.AddPostOptionsLine("EXAMPLES:");
			help.AddPostOptionsLine("  (to keep the examples short, \"Repo\" is used as placeholder for a repository name");
			help.AddPostOptionsLine("   like \"RepoZ\" or \"NSidekick\", for example)");
			help.AddPostOptionsLine("");

			help.AddPostOptionsLine("Basics:");
			help.AddPostOptionsLine("  grr \t\t\tLists all repositories found in RepoZ including their status");
			help.AddPostOptionsLine("  grr list Repo \tShows the status of a given repository (command \"list\" is optional)");
			help.AddPostOptionsLine("  grr cd Repo \t\tNavigates to the main directory of a given repository");
			help.AddPostOptionsLine("  grr open Repo \tOpens the main directory of a given repository (in Windows Explorer)");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("File operations in given repositories:");
			help.AddPostOptionsLine("  grr list Repo *.txt \tLists all text files in the given repository matching the filter *.txt");
			help.AddPostOptionsLine("  grr open Repo *.sln \tOpens the Visual Studio solutions in the given repository");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("RegEx patterns for advanced repository filtering:");
			help.AddPostOptionsLine("  grr list .*_.* \tLists all repositories containing a \"_\"");
			help.AddPostOptionsLine("  grr list \".*[X|Z]\" \tLists all repositories ending with \"X\" or \"Z\"");
			help.AddPostOptionsLine("  grr cd Re.* \t\tNavigates to the first repository starting with \"Re\"");
			help.AddPostOptionsLine("  grr open .*Z *.sln \tOpens each Visual Studio solution in every repository ending with \"Z\"");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("Advanced: grr defines indexes for found repositories. They can be used for the next execution:");
			help.AddPostOptionsLine("  grr list :3 \t\tShows the branch and status of the repository at index 3");
			help.AddPostOptionsLine("  grr open :1 *.sln -e \tOpens the Visual Studio solutions of the repository at index 1 as Admin");
			help.AddPostOptionsLine("  grr list :3 *.* -r \tLists all files of the repository at index 3 recursively");
			help.AddPostOptionsLine("  grr cd :21 \t\tNavigates to the repository at index 21");
			help.AddPostOptionsLine("");
			help.AddPostOptionsLine("Bonus:");
			help.AddPostOptionsLine("  grr cd - \t\tNavigates back to the last path grr was called from");
			help.AddPostOptionsLine("  ");
			help.AddPostOptionsLine("Noteworthy:");
			help.AddPostOptionsLine("  The parameter \"list\" can be omitted, \"grr .*_.*\" has the same effect");
			help.AddPostOptionsLine("  RepoZ has to be running on this system to use grr.");
			help.AddPostOptionsLine("");

			return help.ToString();
		}
	}
}
