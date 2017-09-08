using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Grr
{
	class FilterOptions
	{
		[ValueOption(0)]
		public string Filter { get; set; }
	}

	class CommandLineOptions
	{
		public const string List = "list";
		public const string Goto = "goto";

		[VerbOption(List, HelpText = "Record changes to the repository.")]
		public FilterOptions ListVerb { get; set; }

		[VerbOption(Goto, HelpText = "Update remote refs along with associated objects.")]
		public FilterOptions GotoVerb { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
