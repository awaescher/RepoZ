using RepoZ.Api.Git;
using RepoZ.Api.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RepoZ.Api.Common.Git
{
	public class DefaultRepositoryIgnoreStore : FileRepositoryStore, IRepositoryIgnoreStore
	{
		private List<string> _ignores = null;

		public DefaultRepositoryIgnoreStore(IErrorHandler errorHandler, IAppDataPathProvider appDataPathProvider)
			: base(errorHandler)
		{
			AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
		}

		public override string GetFileName() => Path.Combine(AppDataPathProvider.GetAppDataPath(), "Repositories.ignore");

		public void IgnoreByPath(string path)
		{
			Ignores.Add(path);
			Set(Ignores);
		}

		public bool IsIgnored(string path)
		{
			return Ignores.Contains(path, StringComparer.OrdinalIgnoreCase);
		}

		public void Reset()
		{
			Ignores.Clear();
			Set(Ignores);
		}

		public List<string> Ignores
		{
			get
			{
				if (_ignores == null)
					_ignores = Get()?.ToList() ?? new List<string>();

				return _ignores;
			}
		}

		public IAppDataPathProvider AppDataPathProvider { get; }

		public RegexOptions Options { get; } = RegexOptions.IgnoreCase | RegexOptions.Compiled;
	}
}
