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
        }

        public AutoFetchMode AutoFetchMode
        {
            get => AppSettingsService.AutoFetchMode;
            set
            {
                AppSettingsService.AutoFetchMode = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchMode)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchOff)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchDiscretely)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchAdequate)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchAggressive)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledSearchRepoEverything)));
            }
        }

        public bool AutoFetchOff
        {
            get => AutoFetchMode == AutoFetchMode.Off;
            set => AutoFetchMode = AutoFetchMode.Off;
        }

        public bool AutoFetchDiscretely
        {
            get => AutoFetchMode == AutoFetchMode.Discretely;
            set => AutoFetchMode = AutoFetchMode.Discretely;
        }

        public bool AutoFetchAdequate
        {
            get => AutoFetchMode == AutoFetchMode.Adequate;
            set => AutoFetchMode = AutoFetchMode.Adequate;
        }

        public bool AutoFetchAggressive
        {
            get => AutoFetchMode == AutoFetchMode.Aggressive;
            set => AutoFetchMode = AutoFetchMode.Aggressive;
        }

        public bool PruneOnFetch
        {
            get => AppSettingsService.PruneOnFetch;
            set => AppSettingsService.PruneOnFetch = value;
        }

        public bool EnabledSearchRepoEverything
        {
            get => AppSettingsService.EnabledSearchRepoEverything;
            set => AppSettingsService.EnabledSearchRepoEverything = value;
        }

        public IAppSettingsService AppSettingsService { get; }
    }
}