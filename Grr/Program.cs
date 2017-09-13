﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grr.Messages;
using TinyIpc.Messaging;

namespace Grr
{
	class Program
	{
		private const int MAX_REPO_NAME_LENGTH = 32;

		private static TinyMessageBus _bus;
		private static string _answer = null;
		private static Repository[] _repos = null;

		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			IMessage message = null;

			if (args?.Length == 0)
				args = new string[] { CommandLineOptions.List };

			var knownCommands = new string[] { CommandLineOptions.List, CommandLineOptions.ChangeDirectory };
			if (!knownCommands.Contains(args[0]))
			{
				var l = new List<string>(args);
				l.Insert(0, CommandLineOptions.List);
				args = l.ToArray();
			}

			var options = new CommandLineOptions();
			if (CommandLine.Parser.Default.ParseArguments(args, options, (v, o) => ParseCommandLineOptions(v, o, out message)))
			{
				_bus = new TinyMessageBus("RepoZGrrChannel");
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
				{
					var maxRepoNameLenth = Math.Min(MAX_REPO_NAME_LENGTH, _repos.Max(r => r.Name?.Length ?? 0));
					var maxIndexStringLength = _repos.Length.ToString().Length;

					for (int i = 0; i < _repos.Length; i++)
					{
						string repoName = (_repos[i].Name.Length > MAX_REPO_NAME_LENGTH)
							? _repos[i].Name.Substring(MAX_REPO_NAME_LENGTH)
							: _repos[i].Name;

						Console.Write($" [{i.ToString().PadLeft(maxIndexStringLength)}]  ");
						Console.Write(repoName.PadRight(maxRepoNameLenth + 3));
						Console.Write(_repos[i].BranchWithStatus);
						Console.WriteLine();
					}
				}
				else
				{
					Console.WriteLine(_answer);
				}

				message?.Execute(_repos);

				if (Debugger.IsAttached)
					Console.ReadKey();
			}
			else
			{
				Console.WriteLine("Could not parse command line arguments.");
			}
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

			if (verb == CommandLineOptions.List)
				message = new ListMessage(filter);

			if (verb == CommandLineOptions.ChangeDirectory)
				message = new ChangeDirectoryMessage(filter);
		}
	}
}
