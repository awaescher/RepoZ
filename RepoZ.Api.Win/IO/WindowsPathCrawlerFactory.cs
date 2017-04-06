using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.IO;

namespace RepoZ.Api.Win.IO
{
	public class WindowsPathCrawlerFactory : IPathCrawlerFactory
	{
		public IPathCrawler Create() => new GravellPathCrawler();
	}
}
