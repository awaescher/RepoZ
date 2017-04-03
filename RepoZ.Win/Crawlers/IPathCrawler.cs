using System;
using System.Collections.Generic;

namespace RepoZ.Win.Crawlers
{
	public interface IPathCrawler
	{
		List<string> Find(string root, string searchPattern, Action<string> onFoundAction, Action onQuit);
	}
}