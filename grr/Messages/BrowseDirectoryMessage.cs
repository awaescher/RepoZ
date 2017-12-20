using System.Diagnostics;
using System.IO;
using System.Linq;

namespace grr.Messages
{

	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public class BrowseDirectoryMessage : IMessage
	{
		private readonly string _repositoryFilter;

		public BrowseDirectoryMessage(string repositoryFilter)
		{
			_repositoryFilter = repositoryFilter;
		}

		public void Execute(Repository[] repositories)
		{
			if (repositories == null || repositories.Length <= 0)
				return;

			string path = repositories.First().Path;

			if (string.IsNullOrWhiteSpace(path))
			{
				System.Console.WriteLine("Repository path is empty. Aborting.");
				return;
			}

			if (Directory.Exists(path))
			{
				// do NOT replace \ with / for the Windows Explorer
				Process.Start("explorer.exe", $"\"{path}\"");
			}
			else
			{
				System.Console.WriteLine("Repository path does not exist:\n" + path);
			}
		}

		public string GetRemoteCommand() => string.IsNullOrEmpty(_repositoryFilter)
			? null /* makes no sense */
			: $"list:^{_repositoryFilter}$";

		public bool HasRemoteCommand => !string.IsNullOrEmpty(_repositoryFilter);
		
	}
}
