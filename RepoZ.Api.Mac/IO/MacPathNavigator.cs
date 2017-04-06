using System;
using System.Diagnostics;
using RepoZ.Api.IO;

namespace RepoZ.Api.Mac.IO
{
	public class MacPathNavigator : IPathNavigator
	{
		public MacPathNavigator()
		{
		}

		public void Navigate(string path)
		{
			try
			{
				Process.Start(path);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
