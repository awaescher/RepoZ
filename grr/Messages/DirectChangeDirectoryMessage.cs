using System.Diagnostics;
using System.IO;
using System.Linq;

namespace grr.Messages
{
	public class DirectChangeDirectoryMessage : IMessage
	{
		private readonly string _targetDirectory;

		public DirectChangeDirectoryMessage(string targetDirectory)
		{
			_targetDirectory = targetDirectory;
		}

		public void Execute(Repository[] repositories)
		{
			if (Directory.Exists(_targetDirectory))
			{
				// use '/' for linux systems and bash command line (will work on cmd and powershell as well)
				string path = _targetDirectory.Replace(@"\", "/");

				var command = $"cd \"{path}\"";
				var parentProcess = Process.GetCurrentProcess();
				ConsoleExtensions.WriteConsoleInput(parentProcess, command);
			}
			else
			{
				System.Console.WriteLine("Path does not exist:\n" + _targetDirectory);
			}
		}

		public string GetRemoteCommand() => null;

		public bool HasRemoteCommand => false;

	}
}
