using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.IO
{
	public class WindowsPathCrawler 
	{
		public List<string> Find(string root, string searchPattern)
		{
			var list = new List<string>();

			var sw = System.Diagnostics.Stopwatch.StartNew();
			Find(root, searchPattern, list);
			sw.Stop();

			return list;
		}

		private void Find(string root, string searchPattern, List<string> result)
		{
			if (root.IndexOf("$Recycle.Bin", StringComparison.OrdinalIgnoreCase) > -1)
				return;

			foreach (string file in Directory.GetFiles(root, searchPattern))
				result.Add(file);

			foreach (string subdirectory in Directory.GetDirectories(root))
			{
				try
				{
					Find(subdirectory, searchPattern, result);
				}
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
				catch
#pragma warning restore RECS0022
				{
					// swallow, log, whatever
				}
			}
		}
	}
}
