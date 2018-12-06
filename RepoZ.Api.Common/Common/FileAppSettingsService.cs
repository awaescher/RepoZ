using Newtonsoft.Json;
using RepoZ.Api.Common.Git.AutoFetch;
using RepoZ.Api.IO;
using System;
using System.IO;

namespace RepoZ.Api.Common.Common
{
	public class FileAppSettingsService : IAppSettingsService
	{
		private AppSettings _settings;

		public FileAppSettingsService(IAppDataPathProvider appDataPathProvider)
		{
			AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
		}

		private AppSettings Load()
		{
			string file = GetFileName();

			if (File.Exists(file))
			{
				try
				{
					var json = File.ReadAllText(file);
					return JsonConvert.DeserializeObject<AppSettings>(json);
				}
				catch { }
			}

			return AppSettings.Default;
		}

		private void Save()
		{
			string file = GetFileName();
			string path = Directory.GetParent(file).FullName;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			try
			{
				File.WriteAllText(GetFileName(), JsonConvert.SerializeObject(_settings));
			}
			catch { }
		}

		private string GetFileName() =>  Path.Combine(AppDataPathProvider.GetAppDataPath(), "appsettings.json");

		public IAppDataPathProvider AppDataPathProvider { get; }

		public AppSettings Settings
		{
			get
			{
				if (_settings == null)
					_settings = Load();

				return _settings;
			}
		}

		public AutoFetchMode AutoFetchMode
		{
			get => Settings.AutoFetchMode;
			set
			{
				Settings.AutoFetchMode = value;
				Save();
			}
		}
	}
}