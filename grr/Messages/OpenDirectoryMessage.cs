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
			Process.Start(new ProcessStartInfo($"\"{directory}\"") { UseShellExecute = true });
		}
	}
}
