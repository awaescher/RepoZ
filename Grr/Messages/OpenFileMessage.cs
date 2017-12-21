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
				var psi = new ProcessStartInfo(file);
				Process.Start(psi);
			}
		}
	}
}
