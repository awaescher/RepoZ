using RepoZ.Api.Git;
using RepoZ.Ipc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Terminal.Gui;

namespace grrui
{
	class Program
	{
		private const int MAX_REPO_NAME_LENGTH = 35;

		private static RepozIpcClient _client;
		private static StatusCharacterMap _map;

		static void Main(string[] args)
		{
			_map = new StatusCharacterMap();
			_client = new RepozIpcClient();
			var answer = _client.GetRepositories();

			if (answer.Repositories == null)
			{
				Console.WriteLine(answer.Answer);
				return;
			}

			if (answer.Repositories.Length == 0)
			{
				Console.WriteLine("No repositories yet");
				return;
			}

			Application.Init();

			var list = new ListView(WriteRepositories(answer.Repositories).ToList());

			var win = new Window("Repositories");
			win.Add(list);

			Application.Top.Add(win);
			Application.Run();
		}

		private static IEnumerable<string> WriteRepositories(Repository[] repositories)
		{
			var maxRepoNameLength = Math.Min(MAX_REPO_NAME_LENGTH, repositories.Max(r => r.Name?.Length ?? 0));
			var maxIndexStringLength = repositories.Length.ToString().Length;
			var writeIndex = repositories.Length > 1;

			for (int i = 0; i < repositories.Length; i++)
			{
				var userIndex = i + 1; // the index visible to the user are 1-based, not 0-based;

				string repoName = (repositories[i].Name.Length > MAX_REPO_NAME_LENGTH)
					? repositories[i].Name.Substring(0, MAX_REPO_NAME_LENGTH) + _map.EllipsesSign
					: repositories[i].Name;

				var index = "";
				if (writeIndex)
					index = $"[{userIndex.ToString().PadLeft(maxIndexStringLength)}]  ";

				var name = repoName.PadRight(maxRepoNameLength + 3);
				var branch = repositories[i].BranchWithStatus;

				yield return index + Clean(name) + Clean(branch);
			}
		}

		private static string Clean(string branch)
		{
			// **TODO**
			// Remove this as soon as gui.cs can handle unicode chars
			// see: https://github.com/migueldeicaza/gui.cs/issues/146

			return branch.Replace(_map.ArrowDownSign, "»")
				.Replace(_map.ArrowUpSign, "«")
				.Replace(_map.EllipsesSign, "~")
				.Replace(_map.IdenticalSign, "=")
				.Replace(_map.NoUpstreamSign, "*");
		}
	}
}