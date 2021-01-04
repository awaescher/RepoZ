using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RepoZ.Ipc;

namespace grr.Messages
{
	[DebuggerDisplay("{GetRemoteCommand()}")]
	public class ChangeToDirectoryMessage : DirectoryMessage
	{
		public ChangeToDirectoryMessage(RepositoryFilterOptions filter)
			: base(filter)
		{
		}

		protected override void ExecuteExistingDirectory(string directory)
		{
			var command = $"cd \"{directory}\"";

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// type the path into the console which is hosting grr.exe to change to the directory
				TextCopy.ClipboardService.SetText(command);
				ConsoleExtensions.WriteConsoleInput(Process.GetCurrentProcess(), command);
			}
			else
			{
				TextCopy.ClipboardService.SetText(command);
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("The command was copied to the clipboard, paste and execute it manually now.\nChanging directories is not supported on macOS yet, sorry.");
				Console.ResetColor();
			}
		}

		protected override void ExecuteRepositoryQuery(Repository[] repositories)
		{
			if (repositories?.Length > 1)
			{
				// only use the first repository when multiple repositories came in
				// cd makes no sense with multiple repositories
				System.Console.WriteLine("");
				System.Console.WriteLine($"Found multiple repositories, using {repositories[0].Name}.");
				System.Console.WriteLine("You can access the others by index now, like:\n  grr cd :2");
				base.ExecuteRepositoryQuery(new Repository[] { repositories[0] });
			}
			else
			{
				base.ExecuteRepositoryQuery(repositories);
			}
		}

		public override bool ShouldWriteRepositories(Repository[] repositories) => true;
	}
}
