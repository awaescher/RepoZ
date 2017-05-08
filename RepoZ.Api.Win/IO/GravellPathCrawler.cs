using RepoZ.Api.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.Win.IO
{
	// http://stackoverflow.com/questions/2106877/is-there-a-faster-way-than-this-to-find-all-the-files-in-a-directory-and-all-sub

	public class GravellPathCrawler : IPathCrawler
	{
		public List<string> Find(string root, string searchPattern, Action<string> onFoundAction, Action onQuit)
		{
			return FindInternal(root, searchPattern, onFoundAction, onQuit).ToList();
		}
		private IEnumerable<string> FindInternal(string root, string searchPattern, Action<string> onFound, Action onQuit)
		{
			var pending = new Queue<string>();
			pending.Enqueue(root);
			string[] tmp;
			while (pending.Count > 0)
			{
				root = pending.Dequeue();

				if (root.IndexOf("$Recycle.Bin", StringComparison.OrdinalIgnoreCase) > -1)
					continue;

				if (root.IndexOf("C:\\Windows", StringComparison.OrdinalIgnoreCase) > -1)
					continue;

				if (root.IndexOf("Package Cache", StringComparison.OrdinalIgnoreCase) > -1)
					continue;

				if (root.IndexOf(".nuget", StringComparison.OrdinalIgnoreCase) > -1)
					continue;

				if (root.IndexOf("Local\\Temp", StringComparison.OrdinalIgnoreCase) > -1)
					continue;
				
				try
				{
					tmp = Directory.GetFiles(root, searchPattern);
				}
				catch (Exception)
				{
					continue;
				}

				for (int i = 0; i < tmp.Length; i++)
				{
					onFound?.Invoke(tmp[i]);
					yield return tmp[i];
				}
				tmp = Directory.GetDirectories(root);
				for (int i = 0; i < tmp.Length; i++)
				{
					pending.Enqueue(tmp[i]);
				}
			}

			onQuit?.Invoke();
		}
	}
}
