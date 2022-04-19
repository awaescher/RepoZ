namespace RepoZ.Api.Common.Common
{
    using RepoZ.Api.Common.Git.AutoFetch;

    public class AppSettings
    {
        public AppSettings()
        {
            MenuSize = Size.Default;
        }

        public AutoFetchMode AutoFetchMode { get; set; }

        public bool PruneOnFetch { get; set; }

        public Size MenuSize { get; set; }

        public static AppSettings Default => new AppSettings()
            {
                AutoFetchMode = AutoFetchMode.Off,
                PruneOnFetch = false,
                MenuSize = Size.Default,
            };
    }

    public class Size
    {
        public double Height { get; set; }

        public double Width { get; set; }

        public static Size Default => new Size()
            {
                Width = -1,
                Height = -1,
            };
    }
}