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
			System.Console.WriteLine($"Opening {files[0]} ...");
			var psi = new ProcessStartInfo(files[0]);
			Process.Start(psi);
		}
	}
}
