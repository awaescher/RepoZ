using System.Diagnostics;
using System.IO;
using System.Linq;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public class OpenFileMessage : FileMessage
	{
		public OpenFileMessage(RepositoryFilterOptions filter)
			: base(filter)
		{
		}

		protected override void ExecuteFound(string[] files)
		{
			foreach (var file in files)
			{
				System.Console.WriteLine($"Opening {file} ...");

				try
				{
					Process.Start(CreateStartInfo(file));
				}
				catch (System.Exception ex)  
				{
					System.Console.WriteLine("An error occured:\n" + ex.ToString());
				}
			}
		}

		private ProcessStartInfo CreateStartInfo(string file)
		{
			if (!Filter.RequestElevation)
				return new ProcessStartInfo(file);

			var isExecutable = IsExecutable(file);

			// executables can be used directly, whereas files such as *.sln, for example,
			// have to been opened with (a hidden) cmd.exe to request elevation.
			// command is:   cmd.exe /C "C:\Path\Solution.sln"
			var executable = isExecutable ? file : "cmd.exe";
			var arguments = isExecutable ? "" : $"/C \"{file}\"";
			var windowStyle = isExecutable ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;

			return new ProcessStartInfo
			{
				UseShellExecute = true,		// this with Verb=runas forces elevation
				CreateNoWindow = true,
				WindowStyle = windowStyle,
				FileName = executable,
				Verb = "runas",				// this with ShellEx=true forces elevation
				Arguments = arguments
			};
		}

		private bool IsExecutable(string file)
		{
			var executables = new string[] { ".exe", ".bat", ".cmd", ".com" };
			return executables.Any(e => file.EndsWith(e, System.StringComparison.OrdinalIgnoreCase));
		}
	}
}
