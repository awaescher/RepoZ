using System;
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

			var options = new Options();
			if (CommandLine.Parser.Default.ParseArguments(args, options))
			{
				object message = options.IsListMode
					? new ListMessage(options.ListFilter)
					: null;

				if (message == null && options.IsNavigationMode)
					message = new NavigateMessage(options.NavigateFilter);

				if (message == null)
					message = new ListMessage("*");

				if (message != null)
				{
					_bus = new TinyMessageBus("RepoZGrrChannel");
					_bus.MessageReceived += _bus_MessageReceived;

					byte[] load = Encoding.UTF8.GetBytes(message.ToString());
					_bus.PublishAsync(load);
				}
			}

			while (_answer == null)
			{

			}

			_bus?.Dispose();

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

			if (Debugger.IsAttached)
				Console.ReadKey();
		}

		private static void _bus_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
		{
			var answer = Encoding.UTF8.GetString(e.Message);

			_repos = answer.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
				.Select(s => Repository.FromString(s))
				.ToArray();

			_answer = answer;
		}
	}
}
