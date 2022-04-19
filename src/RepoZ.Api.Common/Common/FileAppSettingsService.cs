namespace RepoZ.Api.Common.Common
{
    using Newtonsoft.Json;
    using RepoZ.Api.Common.Git.AutoFetch;
    using RepoZ.Api.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class FileAppSettingsService : IAppSettingsService
    {
        private AppSettings _settings;
        private readonly List<Action> _invalidationHandlers = new List<Action>();

        public FileAppSettingsService(IAppDataPathProvider appDataPathProvider)
        {
            AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        }

        private AppSettings Load()
        {
            var file = GetFileName();

            if (File.Exists(file))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    return JsonConvert.DeserializeObject<AppSettings>(json);
                }
                catch
                {
                    /* Our app settings are not critical. For our purposes, we want to ignore IO exceptions */
                }
            }

            return AppSettings.Default;
        }

        private void Save()
        {
            var file = GetFileName();
            var path = Directory.GetParent(file).FullName;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                File.WriteAllText(GetFileName(), JsonConvert.SerializeObject(_settings));
            }
            catch
            {
                /* Our app settings are not critical. For our purposes, we want to ignore IO exceptions */
            }
        }

        private string GetFileName()
        {
            return Path.Combine(AppDataPathProvider.GetAppDataPath(), "appsettings.json");
        }

        public IAppDataPathProvider AppDataPathProvider { get; }

        public AppSettings Settings => _settings ?? (_settings = Load());

        public AutoFetchMode AutoFetchMode
        {
            get => Settings.AutoFetchMode;
            set
            {
                if (value != Settings.AutoFetchMode)
                {
                    Settings.AutoFetchMode = value;

                    NotifyChange();
                    Save();
                }
            }
        }

        public bool PruneOnFetch
        {
            get => Settings.PruneOnFetch;
            set
            {
                if (value != Settings.PruneOnFetch)
                {
                    Settings.PruneOnFetch = value;

                    NotifyChange();
                    Save();
                }
            }
        }

        public double MenuWidth
        {
            get => Settings.MenuSize.Width;
            set
            {
                if (value != Settings.MenuSize.Width)
                {
                    Settings.MenuSize.Width = value;

                    NotifyChange();
                    Save();
                }
            }
        }

        public double MenuHeight
        {
            get => Settings.MenuSize.Height;
            set
            {
                if (value != Settings.MenuSize.Height)
                {
                    Settings.MenuSize.Height = value;

                    NotifyChange();
                    Save();
                }
            }
        }

        public void RegisterInvalidationHandler(Action handler)
        {
            _invalidationHandlers.Add(handler);
        }

        public void NotifyChange()
        {
            _invalidationHandlers.ForEach(h => h.Invoke());
        }
    }
}