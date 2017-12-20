using System.IO;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public abstract class FileMessage2 : DirectoryMessage
	{
		public FileMessage2(string argument)
			: base(argument)
		{
		}

		protected override void ExecuteExistingDirectory(string directory)
		{
		}
	}
}
