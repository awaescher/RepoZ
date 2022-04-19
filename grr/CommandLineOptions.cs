namespace grr
{
    using System;
    using System.Linq;
    using CommandLine;
    using CommandLine.Text;

    [Verb("list", HelpText = "(Default) Lists the repositories found by RepoZ including their current branch and the corresponding status. Can be omitted like shown in the examples below.")]
    public class ListOptions : RepositoryFilterOptions { }

    [Verb("cd", HelpText = "Causes the command line interface to navigate to the directory of a given repository.")]
    public class ChangeDirectoryOptions : RepositoryFilterOptions { }

    [Verb("gd", HelpText = "Returns the main directory of a given repository and puts it into the clipboard")]
    public class GetDirectoryOptions : RepositoryFilterOptions { }

    [Verb("open", HelpText = "Opens the directory or a file of a given repository with the operating system's default application.")]
    public class OpenDirectoryOptions : RepositoryFilterOptions { }

    partial class CommandLineOptions
    {
        public const string LIST_COMMAND = "list";
        public const string CHANGE_DIRECTORY_COMMAND = "cd";
        public const string GET_DIRECTORY_COMMAND = "gd";
        public const string OPEN_DIRECTORY_COMMAND = "open";

        public const string HELP_COMMAND = "help";
        public const char HELP_COMMAND_CHAR = '?';

        public static string[] GetKnownCommands()
        {
            return new string[]
                {
                    LIST_COMMAND,
                    CHANGE_DIRECTORY_COMMAND,
                    GET_DIRECTORY_COMMAND,
                    OPEN_DIRECTORY_COMMAND,
                    HELP_COMMAND,
                    HELP_COMMAND_CHAR.ToString(),
                };
        }

        public static bool IsKnownArgument(string arg)
        {
            arg = arg.TrimStart('-').TrimStart('/');
            return GetKnownCommands().Contains(arg, StringComparer.OrdinalIgnoreCase);
        }

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

            var knownCommandsPiped = string.Join("|", GetKnownCommands());
            help.AddPreOptionsLine(" ");
            help.AddPreOptionsLine(" ");
            help.AddPreOptionsLine("USAGE:");
            help.AddPreOptionsLine($"  grr [{knownCommandsPiped}] [repository filter or RegEx pattern] [file filter] [file options]");
            help.AddPreOptionsLine(" ");
            help.AddPreOptionsLine(" ");
            help.AddPreOptionsLine(" ");

            help.AddPreOptionsLine("COMMANDS:");
            help.AddVerbs(typeof(ListOptions), typeof(ChangeDirectoryOptions), typeof(OpenDirectoryOptions));

            help.AddPostOptionsLine(" ");
            help.AddPostOptionsLine("FILTERS:");
            help.AddPostOptionsLine("⁞  Repository filter or RegEx pattern:");
            help.AddPostOptionsLine("⁞    The filter pattern to find matching repositories with a like search.");
            help.AddPostOptionsLine("⁞    If a like search is too broad, use a RegEx pattern instead by adding square brackets.");
            help.AddPostOptionsLine("⁞    Like [.*Z] for all repositories ending with \"Z\".");
            help.AddPostOptionsLine("⁞    Note that you should put the filter or RegEx pattern in quotes if it contains spaces.");
            help.AddPostOptionsLine("⁞");
            help.AddPostOptionsLine("⁞  File name or filter:");
            help.AddPostOptionsLine("⁞    The filter pattern to find matching files of a given repository with a like search.");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("OPTIONS:");
            help.AddPostOptionsLine("⁞  --recursive or -r:");
            help.AddPostOptionsLine("⁞    Enables recursive search in subdirectories of a given Git repository.");
            help.AddPostOptionsLine("⁞    Compatible with commands: \"list\" and \"open\"");
            help.AddPostOptionsLine("⁞");
            help.AddPostOptionsLine("⁞  --elevated or -e:");
            help.AddPostOptionsLine("⁞    Invokes the UAC dialog to request elevated priviledges for the process to open.");
            help.AddPostOptionsLine("⁞    Compatible with commands: \"open\"");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("");

            help.AddPostOptionsLine("EXAMPLES:");
            help.AddPostOptionsLine("⁞  (to keep the examples short, \"Repo\" is used as placeholder for a repository name");
            help.AddPostOptionsLine("⁞   like \"RepoZ\" or \"NSidekick\", for example)");
            help.AddPostOptionsLine("");

            help.AddPostOptionsLine("Basics:");
            help.AddPostOptionsLine("⁞  grr \t\t\tLists all repositories found in RepoZ including their status");
            help.AddPostOptionsLine("⁞  grr list Repo\tShows the status of a given repository (command \"list\" is optional)");
            help.AddPostOptionsLine("⁞  grr cd Repo\t\tNavigates to the main directory of a given repository");
            help.AddPostOptionsLine("⁞  grr gd Repo\t\tReturns the main directory of a given repository and puts it into the clipboard");
            help.AddPostOptionsLine("⁞  grr open Repo\tOpens the main directory of a given repository (in Windows Explorer)");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("Predefined filters:");
            help.AddPostOptionsLine("⁞  grr todo\t\tLists repositories with unpushed changes (file changes, stashes and more)");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("Filter targets:");
            help.AddPostOptionsLine("⁞  grr Repo\t\tBy default, filters are applied to the repository name");
            help.AddPostOptionsLine("⁞  grr \"n Repo\"\t\tThe prefix \"n \" forces RepoZ to filter for repository names (optional)");
            help.AddPostOptionsLine("⁞  grr \"b master\"\tThe prefix \"b \" forces RepoZ to filter for repository branches");
            help.AddPostOptionsLine("⁞  grr \"p C:\\\"\t\tThe prefix \"p \" forces RepoZ to filter for repository paths");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("File operations in given repositories:");
            help.AddPostOptionsLine("⁞  grr list Repo *.txt\tLists all text files in the given repository matching the filter *.txt");
            help.AddPostOptionsLine("⁞  grr open Repo *.sln\tOpens the Visual Studio solutions in the given repository");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("RegEx patterns for advanced repository filtering (note the square brackets):");
            help.AddPostOptionsLine("⁞  grr list [.*_.*]\tLists all repositories containing a \"_\"");
            help.AddPostOptionsLine("⁞  grr cd [Re.*]\tNavigates to the first repository starting with \"Re\"");
            help.AddPostOptionsLine("⁞  grr open [.*Z] *.sln\tOpens each Visual Studio solution in every repository ending with \"Z\"");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("Advanced: grr defines indexes for found repositories. They can be used for the next execution:");
            help.AddPostOptionsLine("⁞  grr list :3 \t\tShows the branch and status of the repository at index 3");
            help.AddPostOptionsLine("⁞  grr open :1 *.sln -e\tOpens the Visual Studio solutions of the repository at index 1 as Admin");
            help.AddPostOptionsLine("⁞  grr list :3 *.* -r\tLists all files of the repository at index 3 recursively");
            help.AddPostOptionsLine("⁞  grr cd :21 \t\tNavigates to the repository at index 21");
            help.AddPostOptionsLine("");
            help.AddPostOptionsLine("Bonus:");
            help.AddPostOptionsLine("⁞  grr cd - \t\tNavigates back to the last path grr was called from");
            help.AddPostOptionsLine("  ");
            help.AddPostOptionsLine("Noteworthy:");
            help.AddPostOptionsLine("⁞  The parameter \"list\" can be omitted, \"grr [.*_.*]\" has the same effect.");
            help.AddPostOptionsLine("⁞  Put your filter in quotes if it contains spaces.");
            help.AddPostOptionsLine("⁞  RepoZ has to be running on this system to use grr.");
            help.AddPostOptionsLine("");

            return help.ToString();
        }
    }
}