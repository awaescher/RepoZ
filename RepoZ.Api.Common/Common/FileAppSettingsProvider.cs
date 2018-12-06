using Newtonsoft.Json;
using RepoZ.Api.IO;
using System;
using System.IO;

namespace RepoZ.Api.Common.Common
{
	public class FileAppSettingsProvider : IAppSettingsProvider
	{
		public FileAppSettingsProvider(IAppDataPathProvider appDataPathProvider)
		{
			AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
		}

		public IAppDataPathProvider AppDataPathProvider { get; }

		public AppSettings Load()
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

		public void Save(AppSettings settings)
		{
			string file = GetFileName();
			string path = Directory.GetParent(file).FullName;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			try
			{
				File.WriteAllText(GetFileName(), JsonConvert.SerializeObject(settings));
			}
			catch { }
		}

		private string GetFileName() =>  Path.Combine(AppDataPathProvider.GetAppDataPath(), "appsettings.json");
	}
}