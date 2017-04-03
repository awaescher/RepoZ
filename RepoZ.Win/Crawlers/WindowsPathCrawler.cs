using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Win.Crawlers
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
				catch
				{
					// swallow, log, whatever
				}
			}
		}
	}
}
