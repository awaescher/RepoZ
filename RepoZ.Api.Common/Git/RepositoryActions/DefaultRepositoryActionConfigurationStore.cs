using Newtonsoft.Json;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryActionConfigurationStore : FileRepositoryStore, IRepositoryActionConfigurationStore
	{
		private RepositoryActionConfiguration _repositoryActions = null;
		private object _lock = new object();

		public DefaultRepositoryActionConfigurationStore(IErrorHandler errorHandler, IAppDataPathProvider appDataPathProvider)
			: base(errorHandler)
		{
			AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
		}

		public override string GetFileName() => Path.Combine(AppDataPathProvider.GetAppDataPath(), "RepositoryActions.json");

		public RepositoryActionConfiguration RepositoryActionConfiguration
		{
			get
			{
				if (_repositoryActions == null)
				{
					lock (_lock)
					{
						var lines = Get()?.ToList() ?? new List<string>();
						var json = string.Join(Environment.NewLine, lines.Select(RemoveComment));
						_repositoryActions = JsonConvert.DeserializeObject<RepositoryActionConfiguration>(json);
					}
				}

				return _repositoryActions;
			}
		}

		private string RemoveComment(string line)
		{
			var indexOfComment = line.IndexOf('#');
			return indexOfComment < 0 ? line : line.Substring(0, indexOfComment);
		}

		public IAppDataPathProvider AppDataPathProvider { get; }
	}
}
