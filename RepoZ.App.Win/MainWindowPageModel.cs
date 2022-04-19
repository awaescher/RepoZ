namespace RepoZ.App.Win
{
    using RepoZ.Api.Common.Common;
    using RepoZ.Api.Common.Git.AutoFetch;
    using System;
    using System.ComponentModel;

    public class MainWindowPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowPageModel(IAppSettingsService appSettingsService)
        {
            AppSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchMode)));
        }

        public AutoFetchMode AutoFetchMode
        {
            get
            {
                return AppSettingsService.AutoFetchMode;
            }
            set
            {
                AppSettingsService.AutoFetchMode = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchMode)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchOff)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchDiscretely)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchAdequate)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchAggresive)));
            }
        }

        public bool AutoFetchOff
        {
            get
            {
                return AutoFetchMode == AutoFetchMode.Off;
            }
            set
            {
                AutoFetchMode = AutoFetchMode.Off;
            }
        }

        public bool AutoFetchDiscretely
        {
            get
            {
                return AutoFetchMode == AutoFetchMode.Discretely;
            }
            set
            {
                AutoFetchMode = AutoFetchMode.Discretely;
            }
        }

        public bool AutoFetchAdequate
        {
            get
            {
                return AutoFetchMode == AutoFetchMode.Adequate;
            }
            set
            {
                AutoFetchMode = AutoFetchMode.Adequate;
            }
        }

        public bool AutoFetchAggresive
        {
            get
            {
                return AutoFetchMode == AutoFetchMode.Aggresive;
            }
            set
            {
                AutoFetchMode = AutoFetchMode.Aggresive;
            }
        }

        public bool PruneOnFetch
        {
            get
            {
                return AppSettingsService.PruneOnFetch;
            }
            set
            {
                AppSettingsService.PruneOnFetch = value;
            }
        }

        public IAppSettingsService AppSettingsService { get; }
    }
}