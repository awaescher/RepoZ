namespace RepoZ.Api.Common.Common
{
    using System.Collections.Generic;
    using RepoZ.Api.Common.Git.AutoFetch;

    public class AppSettings
    {
        public AppSettings()
        {
            MenuSize = Size.Default;
            EnabledSearchProviders = new List<string>();
        }

        public AutoFetchMode AutoFetchMode { get; set; }

        public bool PruneOnFetch { get; set; }

        public Size MenuSize { get; set; }

        public List<string> EnabledSearchProviders { get; set; }

        public static AppSettings Default => new()
            {
                AutoFetchMode = AutoFetchMode.Off,
                PruneOnFetch = false,
                MenuSize = Size.Default,
                EnabledSearchProviders = new List<string>(1),
            };
    }

    public class Size
    {
        public double Height { get; set; }

        public double Width { get; set; }

        public static Size Default => new()
            {
                Width = -1,
                Height = -1,
            };
    }
}