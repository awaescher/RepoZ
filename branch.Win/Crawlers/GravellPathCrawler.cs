using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace branch.Win.Crawlers
{
	// http://stackoverflow.com/questions/2106877/is-there-a-faster-way-than-this-to-find-all-the-files-in-a-directory-and-all-sub

	public class GravellPathCrawler
	{
		public List<string> Find(string root, string searchPattern, Action<string> onFoundAction)
		{
			return FindInternal(root, searchPattern, onFoundAction).ToList();
		}
		private IEnumerable<string> FindInternal(string root, string searchPattern, Action<string> onFoundAction)
		{
			var pending = new Queue<string>();
			pending.Enqueue(root);
			string[] tmp;
			while (pending.Count > 0)
			{
				root = pending.Dequeue();

				if (root.IndexOf("$Recycle.Bin", StringComparison.OrdinalIgnoreCase) > -1)
					continue;

				try
				{
					tmp = Directory.GetFiles(root, searchPattern);
				}
				catch (UnauthorizedAccessException)
				{
					continue;
				}
				for (int i = 0; i < tmp.Length; i++)
				{
					onFoundAction?.Invoke(tmp[i]);
					yield return tmp[i];
				}
				tmp = Directory.GetDirectories(root);
				for (int i = 0; i < tmp.Length; i++)
				{
					pending.Enqueue(tmp[i]);
				}
			}
		}
	}
}
