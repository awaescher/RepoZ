using System;
using System.Collections.Generic;

namespace RepoZ.Api.IO
{
	public interface IPathCrawler
	{
		List<string> Find(string root, string searchPattern, Action<string> onFoundAction, Action onQuit);
	}
}