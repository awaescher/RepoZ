using Newtonsoft.Json;
using RepoZ.Api.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryActionConfigurationStore : FileRepositoryStore, IRepositoryActionConfigurationStore
	{
		private object _lock = new object();

		public DefaultRepositoryActionConfigurationStore(IErrorHandler errorHandler, IAppDataPathProvider appDataPathProvider)
			: base(errorHandler)
		{
			AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
		}

		public override string GetFileName() => Path.Combine(AppDataPathProvider.GetAppDataPath(), "RepositoryActions.json");

		public void Preload()
		{
			lock (_lock)
			{
				if (!File.Exists(GetFileName()))
				{
					if (!TryCopyDefaultJsonFile())
					{
						RepositoryActionConfiguration = new RepositoryActionConfiguration();
						RepositoryActionConfiguration.State = RepositoryActionConfiguration.LoadState.None;
						return;
					}
				}

				try
				{
					var lines = Get()?.ToList() ?? new List<string>();
					var json = string.Join(Environment.NewLine, lines.Select(RemoveComment));
					RepositoryActionConfiguration = JsonConvert.DeserializeObject<RepositoryActionConfiguration>(json) ?? new RepositoryActionConfiguration();
					RepositoryActionConfiguration.State = RepositoryActionConfiguration.LoadState.Ok;
				}
				catch (Exception ex)
				{
					RepositoryActionConfiguration = new RepositoryActionConfiguration();
					RepositoryActionConfiguration.State = RepositoryActionConfiguration.LoadState.Error;
					RepositoryActionConfiguration.LoadError = ex.Message;
				}
			}
		}

		private bool TryCopyDefaultJsonFile()
		{
			var defaultFile = Path.Combine(AppDataPathProvider.GetAppResourcesPath(), "RepositoryActions.json");
			var targetFile = GetFileName();

			try
			{
				File.Copy(defaultFile, targetFile);
			}
			catch { /* lets ignore errors here, we just want to know if if worked or not by checking the file existence */ }

			return File.Exists(targetFile);
		}

		private string RemoveComment(string line)
		{
			var indexOfComment = line.IndexOf('#');
			return indexOfComment < 0 ? line : line.Substring(0, indexOfComment);
		}

		public RepositoryActionConfiguration RepositoryActionConfiguration { get; private set; }

		public IAppDataPathProvider AppDataPathProvider { get; }
	}
}
