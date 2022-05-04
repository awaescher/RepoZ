namespace RepoZ.Api.Common.Common
{
    using System;
    using RepoZ.Api.Common.Git.AutoFetch;

    public interface IAppSettingsService
    {
        AutoFetchMode AutoFetchMode { get; set; }

        bool PruneOnFetch { get; set; }

        double MenuWidth { get; set; }

        double MenuHeight { get; set; }

        bool EnabledSearchRepoEverything { get; set; }

        void RegisterInvalidationHandler(Action handler);
    }
}