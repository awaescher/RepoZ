using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Grr.Messages
{

	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public class ChangeDirectoryMessage : IMessage
	{
		private readonly string _repositoryFilter;

		public ChangeDirectoryMessage(string repositoryFilter)
		{
			_repositoryFilter = repositoryFilter;
		}

		public void Execute(Repository[] repositories)
		{
			string path = repositories?.FirstOrDefault()?.Path ?? "";
			if (Directory.Exists(path))
			{
				var command = $"cd \"{path}\"";
				var parentProcess = Process.GetCurrentProcess().Parent();
				ConsoleExtensions.WriteConsoleInput(parentProcess, command);
			}
		}

		public string GetRemoteCommand() => string.IsNullOrEmpty(_repositoryFilter)
			? null /* makes no sense */
			: $"list:^{_repositoryFilter}$";

	}
}
