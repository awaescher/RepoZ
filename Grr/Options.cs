using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Grr
{
	class Options
	{
		[Option('l', "list", DefaultValue = "", HelpText = "Lists all available repositories.")]
		public string ListFilter { get; set; }

		[Option('n', "navigate", DefaultValue = "", HelpText = "Navigates to the matching repository.")]
		public string NavigateFilter { get; set; }

		public bool IsListMode => !string.IsNullOrWhiteSpace(ListFilter);

		public bool IsNavigationMode => !string.IsNullOrWhiteSpace(NavigateFilter);

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
