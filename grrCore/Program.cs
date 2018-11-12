using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using grr.Messages;
using TinyIpc.Messaging;
using grr.Messages.Filters;
using System.IO;

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

			args = PrepareArguments(args);

			if (IsHelpRequested(args))
			{
				ShowHelp();
			}
			else
			{
				var message = TryParseArgumentsToMessage(args);

				if (message != null)
				{
					if (message.HasRemoteCommand)
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

						if (_repos != null && _repos.Any())
							WriteRepositories();
						else
							Console.WriteLine(_answer);
					}

					message?.Execute(_repos);

					WriteHistory();
				}
				else
				{
					Console.WriteLine("Could not parse command line arguments.");
				}
			}

			if (Debugger.IsAttached)
				Console.ReadKey();
		}

		private static IMessage TryParseArgumentsToMessage(string[] args)
		{
			try
			{
				var parseResult = CommandLine.Parser.Default.ParseArguments(args, typeof(ListOptions), typeof(ChangeDirectoryOptions), typeof(OpenDirectoryOptions));
				var options = parseResult.GetType().GetProperty("Value").GetValue(parseResult);
				return GetMessage(options as RepositoryFilterOptions);
			}
			catch
			{
				return null;
			}
		}

		private static void WriteHistory()
		{
			var history = new History.State()
			{
				LastLocation = FindCallerWorkingDirectory(),
				LastRepositories = _repos,
				OverwriteRepositories = (_repos?.Length > 1) /* 0 or 1 repo should not overwrite the last list */
			};

			var repository = new History.RegistryHistoryRepository();
			repository.Save(history);
		}

		private static string FindCallerWorkingDirectory()
		{
			// do NOT use the directory of the grr-assembly
			// we need to preserve the context of the calling console
			return System.IO.Directory.GetCurrentDirectory();
		}

		private static void WriteRepositories()
		{
			var maxRepoNameLength = Math.Min(MAX_REPO_NAME_LENGTH, _repos.Max(r => r.Name?.Length ?? 0));
			var maxIndexStringLength = _repos.Length.ToString().Length;
			var ellipsesSign = "\u2026";
			var writeIndex = _repos.Length > 1;

			for (int i = 0; i < _repos.Length; i++)
			{
				var userIndex = i + 1; // the index visible to the user are 1-based, not 0-based;

				string repoName = (_repos[i].Name.Length > MAX_REPO_NAME_LENGTH)
					? _repos[i].Name.Substring(0, MAX_REPO_NAME_LENGTH) + ellipsesSign
					: _repos[i].Name;

				Console.Write(" ");
				if (writeIndex)
					Console.Write($" [{userIndex.ToString().PadLeft(maxIndexStringLength)}]  ");
				Console.Write(repoName.PadRight(maxRepoNameLength + 3));
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

		private static IMessage GetMessage(RepositoryFilterOptions options)
		{
			// default should be listing all repositories
			IMessage message = new ListRepositoriesMessage();

			ApplyMessageFilters(options);

			if (options is ListOptions)
			{
				if (options.HasFileFilter)
					message = new ListRepositoryFilesMessage(options);
				else
					message = new ListRepositoriesMessage(options);
			}

			if (options is ChangeDirectoryOptions)
				message = new ChangeToDirectoryMessage(options);

			if (options is OpenDirectoryOptions)
			{
				if (options.HasFileFilter)
					message = new OpenFileMessage(options);
				else
					message = new OpenDirectoryMessage(options);
			}

			return message;
		}

		private static void ApplyMessageFilters(RepositoryFilterOptions filter)
		{
			var historyRepository = new History.RegistryHistoryRepository();
			var filters = new IMessageFilter[]
			{
				new IndexMessageFilter(historyRepository),
				new GoBackMessageFilter(historyRepository)
			};

			foreach (var messageFilter in filters)
				messageFilter.Filter(filter);
		}

		private static bool IsHelpRequested(string[] args)
		{
			if (args.Length != 1)
				return false;

			var arg = args[0].TrimStart('-').TrimStart('/');

			return CommandLineOptions.HelpCommand.Equals(arg, StringComparison.OrdinalIgnoreCase)
				|| CommandLineOptions.HelpCommandChar.ToString().Equals(arg, StringComparison.OrdinalIgnoreCase);
		}

		private static void ShowHelp()
		{
			Console.WriteLine(CommandLineOptions.GetUsage());
		}
	}
}
