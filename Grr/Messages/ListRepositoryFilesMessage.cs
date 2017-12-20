using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public class ListRepositoryFilesMessage : FileMessage
	{
		public ListRepositoryFilesMessage(string repositoryArgument, string fileArgument)
			: base(repositoryArgument, fileArgument)
		{
		}

		protected override void ExecuteFound(string[] files)
		{
			foreach (var file in files)
			{
				System.Console.WriteLine(file);
			}
		}

		protected override IEnumerable<string> FindItems(string directory, string filter)
		{
			return Directory.GetFileSystemEntries(directory, filter)
				.OrderBy(i => i);
		}
	}
}
