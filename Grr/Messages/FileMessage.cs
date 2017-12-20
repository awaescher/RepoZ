using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace grr.Messages
{
	[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
	public abstract class FileMessage : DirectoryMessage
	{
		private string _fileFilter;

		public FileMessage(string repositoryFilter, string fileFilter)
			: base(repositoryFilter)
		{
			_fileFilter = fileFilter;
		}

		protected override void ExecuteExistingDirectory(string directory)
		{
			string[] items = null;

			try
			{
				items = FindItems(directory, _fileFilter).ToArray();
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occured:\n" + ex.ToString());
				return;
			}

			if (items == null || items.Length == 0)
			{
				System.Console.WriteLine($"No files found.\n  Directory:\t{directory}\n  Filter:\t{_fileFilter}");
				return;
			}

			ExecuteFound(items);
		}

		protected virtual IEnumerable<string> FindItems(string directory, string filter)
		{
			return Directory.GetFiles(directory, filter)
				.OrderBy(i => i);
		}

		protected abstract void ExecuteFound(string[] files);
	}
}
