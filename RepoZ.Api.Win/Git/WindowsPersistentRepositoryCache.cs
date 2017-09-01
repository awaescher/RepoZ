using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Git;

namespace RepoZ.Api.Win.Git
{
	public class WindowsPersistentRepositoryCache : PersistentFileRepositoryCache
	{
		public WindowsPersistentRepositoryCache(IErrorHandler errorHandler)
			: base(errorHandler)
		{

		}

		public override string GetFileName() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoZ\\Repositories.cache");
	}
}
