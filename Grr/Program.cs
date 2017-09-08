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

			var options = new CommandLineOptions();
			if (CommandLine.Parser.Default.ParseArguments(args, options, (v, o) => ParseCommandLineOptions(v, o, out message)))
			{
				if (message == null)
					message = new ListMessage("*");

				if (message != null)
				{
					_bus = new TinyMessageBus("RepoZGrrChannel");
					_bus.MessageReceived += _bus_MessageReceived;

					byte[] load = Encoding.UTF8.GetBytes(message.GetRemoteCommand());
					_bus.PublishAsync(load);
				}

				var watch = Stopwatch.StartNew();

				while (_answer == null && watch.ElapsedMilliseconds <= 3000)
				{
					// ... wait ...
				}

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

				message?.Execute(_repos);

				if (Debugger.IsAttached)
					Console.ReadKey();
			}

		}

		private static void _bus_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
		{
			var answer = Encoding.UTF8.GetString(e.Message);

			_repos = answer.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
				.Select(s => Repository.FromString(s))
				.Where(r => r != null)
				.ToArray();

			_answer = answer;
		}

		private static void ParseCommandLineOptions(string verb, object options, out IMessage message)
		{
			// default should be listing all repositories
			message = new ListMessage("");

			if (verb == CommandLineOptions.List)
				message = new ListMessage((options as FilterOptions)?.Filter);

			if (verb == CommandLineOptions.Goto)
				message = new GotoMessage((options as FilterOptions)?.Filter);
		}
	}
}