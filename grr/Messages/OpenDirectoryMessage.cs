using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public class OpenDirectoryMessage : DirectoryMessage
	{
		public OpenDirectoryMessage(RepositoryFilterOptions filter)
			: base(filter)
		{
		}

		protected override void ExecuteExistingDirectory(string directory)
		{
			// use '/' for linux systems and bash command line (will work on cmd and powershell as well)
			directory = directory.Replace(@"\", "/");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                directory = $"\"{directory}\"";

            Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
		}
	}
}
