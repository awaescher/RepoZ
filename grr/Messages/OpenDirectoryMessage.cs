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
            var directoryInQuotes = $"\"{directory}\"";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo(directoryInQuotes) { UseShellExecute = true });
            else
                Process.Start(new ProcessStartInfo("open", directoryInQuotes));
        }
    }
}
