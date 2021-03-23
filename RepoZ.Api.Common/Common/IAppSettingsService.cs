using System;
using System.Collections.Generic;
using RepoZ.Api.Common.Git.AutoFetch;

namespace RepoZ.Api.Common.Common
{
	public interface IAppSettingsService
	{
		AutoFetchMode AutoFetchMode { get; set; }

		bool PruneOnFetch { get; set; }

		List<ApplicationPath> ExePaths { get; set; } 

		void RegisterInvalidationHandler(Action handler);
	}
}
