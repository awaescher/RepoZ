using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Grr
{
	class CommandLineOptions
	{
		public const string List = "list";
		public const string ChangeDirectory = "cd";

		[VerbOption(List, HelpText = "Lists the repositories found by RepoZ including their current branch and its state.")]
		public FilterOptions ListVerb { get; set; }

		[VerbOption(ChangeDirectory, HelpText = "Changes to the directory of a given repository.")]
		public FilterOptions ChangeDirectoryVerb { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}

		internal class FilterOptions
		{
			[ValueOption(0)]
			public string Filter { get; set; }

		}
	}
}
