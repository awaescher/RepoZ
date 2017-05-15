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
	public class WindowsRepositoryCache : FileRepositoryCache
	{
		public WindowsRepositoryCache(IErrorHandler errorHandler)
			: base(errorHandler)
		{

		}

		public override string GetFileName() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RepoZ\\Repositories.cache");
	}
}
