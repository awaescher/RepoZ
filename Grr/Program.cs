using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using grr.Messages;
using TinyIpc.Messaging;

namespace grr
{
	static class Program
	{
		private const int MAX_REPO_NAME_LENGTH = 35;

		private static TinyMessageBus _bus;
		private static string _answer = null;
		private static Repository[] _repos = null;

		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			IMessage message = null;

			args = PrepareArguments(args);

			if (IsHelpRequested(args))
			{
				ShowHelp();
			}
			else
			{
				if (CommandLine.Parser.Default.ParseArguments(args, new CommandLineOptions(), 
					(v, o) => ParseCommandLineOptions(v, o, out message)))
				{
					_bus = new TinyMessageBus("RepoZ-ipc");
					_bus.MessageReceived += _bus_MessageReceived;

					byte[] load = Encoding.UTF8.GetBytes(message.GetRemoteCommand());
					_bus.PublishAsync(load);

					var watch = Stopwatch.StartNew();

					while (_answer == null && watch.ElapsedMilliseconds <= 3000)
					{ /* ... wait ... */ }

					if (_answer == null)
						Console.WriteLine("RepoZ seems not to be running :(");

					_bus?.Dispose();

					if (_repos?.Any() ?? false)
						WriteRepositories();
					else
						Console.WriteLine(_answer);

					message?.Execute(_repos);
				}
				else
				{
					Console.WriteLine("Could not parse command line arguments.");
				}
			}

			if (Debugger.IsAttached)
				Console.ReadKey();
		}

		private static void WriteRepositories()
		{
			var maxRepoNameLenhth = Math.Min(MAX_REPO_NAME_LENGTH, _repos.Max(r => r.Name?.Length ?? 0));
			var maxIndexStringLength = _repos.Length.ToString().Length;
			var ellipsesSign = "\u2026";

			for (int i = 0; i < _repos.Length; i++)
			{
				string repoName = (_repos[i].Name.Length > MAX_REPO_NAME_LENGTH)
					? _repos[i].Name.Substring(0, MAX_REPO_NAME_LENGTH) + ellipsesSign
					: _repos[i].Name;

				Console.Write($" [{i.ToString().PadLeft(maxIndexStringLength)}]  ");
				Console.Write(repoName.PadRight(maxRepoNameLenhth + 3));
				Console.Write(_repos[i].BranchWithStatus);
				Console.WriteLine();
			}
		}

		private static string[] PrepareArguments(string[] args)
		{
			if (args?.Length == 0)
				args = new string[] { CommandLineOptions.ListCommand };

			if (!CommandLineOptions.IsKnownArgument(args.First()))
			{
				var newArgs = new List<string>(args);
				newArgs.Insert(0, CommandLineOptions.ListCommand);
				args = newArgs.ToArray();
			}

			return args;
		}

		private static void _bus_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
		{
			var answer = Encoding.UTF8.GetString(e.Message);

			_repos = answer.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
				.Select(s => Repository.FromString(s))
				.Where(r => r != null)
				.OrderBy(r => r.Name)
				.ToArray();

			_answer = answer;
		}

		private static void ParseCommandLineOptions(string verb, object options, out IMessage message)
		{
			// default should be listing all repositories
			message = new ListMessage("");

			string filter = (options as CommandLineOptions.FilterOptions)?.Filter;

			if (verb == CommandLineOptions.ListCommand)
				message = new ListMessage(filter);

			if (verb == CommandLineOptions.ChangeDirectoryCommand)
				message = new ChangeDirectoryMessage(filter);
		}

		private static bool IsHelpRequested(string[] args)
		{
			return args.Length == 1 && CommandLineOptions.HelpCommand.Equals(args[0], StringComparison.OrdinalIgnoreCase);
		}

		private static void ShowHelp()
		{
			Console.WriteLine(CommandLineOptions.GetUsage());
		}
	}
}
