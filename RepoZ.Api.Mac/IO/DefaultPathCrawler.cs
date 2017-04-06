using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.IO;

namespace RepoZ.Api.Mac.IO
{
	public class DefaultPathCrawler// : IPathCrawler
	{
		public List<string> Find(string root, string searchPattern, Action<string> onFoundAction, Action onQuit)
		{
			var list = new List<string>();

			var sw = System.Diagnostics.Stopwatch.StartNew();
			Find(root, searchPattern, list, onFoundAction, onQuit);
			sw.Stop();

			return list;
		}

		private void Find(string root, string searchPattern, List<string> result, Action<string> onFoundAction, Action onQuit)
		{
			if (root.IndexOf("$Recycle.Bin", StringComparison.OrdinalIgnoreCase) > -1)
				return;



			foreach (string file in Directory.GetFiles(root, searchPattern))
			{
				onFoundAction?.Invoke(file);
				result.Add(file);
			}				

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
			
			foreach (string subdirectory in Directory.GetDirectories(root))
			{
				try
				{
					Find(subdirectory, searchPattern, result, onFoundAction, onQuit);
				}
				catch (UnauthorizedAccessException)
				{
					continue;
				}
				catch (Exception ex)
				{
					// swallow, log, whatever
				}
			}

#pragma warning restore RECS0022
			

			onQuit?.Invoke();
		}
	}
}
