using System.Diagnostics;
using System.IO;
using System.Linq;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public class OpenFileMessage : FileMessage2
	{
		public OpenFileMessage(string argument)
			: base(argument)
		{
		}
	}
}
