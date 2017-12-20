using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public abstract class FileMessage : DirectoryMessage
	{
		private string _fileArgument;

		public FileMessage(string repositoryArgument, string fileArgument)
			: base(repositoryArgument)
		{
			_fileArgument = fileArgument;
		}

		protected override void ExecuteExistingDirectory(string directory)
		{
			var files = Directory.GetFiles(directory, _fileArgument);

			if (!files.Any())
			{
				System.Console.WriteLine($"No files found.\n  Directory:\t{directory}\n  Filter:\t{_fileArgument}");
				return;
			}

			ExecuteFoundFiles(files);
		}

		protected abstract void ExecuteFoundFiles(string[] files);
	}
}
