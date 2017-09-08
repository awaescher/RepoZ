using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grr.Messages
{
	public class GotoMessage : IMessage
	{
		private readonly string _repositoryFilter;

		public GotoMessage(string repositoryFilter)
		{
			_repositoryFilter = repositoryFilter;
		}

		public void Execute(Repository[] repositories)
		{
			string path = repositories?.FirstOrDefault()?.Path ?? "";
			if (Directory.Exists(path))
			{
				string arguments = $"/k \"cd {path}\"";
				Console.WriteLine(arguments);
				Process.Start("cmd.exe", arguments);
			}
		}

		public string GetRemoteCommand() => string.IsNullOrEmpty(_repositoryFilter)
			? null /* makes no sense */
			: $"list:{_repositoryFilter}";

		public override string ToString() => string.IsNullOrEmpty(_repositoryFilter) 
			? null /* makes no sense */
			: $"navigate:{_repositoryFilter}";

	}
}
