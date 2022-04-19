namespace RepoZ.Api.Win.IO
{
    using RepoZ.Api.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WindowsPathSkipper : IPathSkipper
    {
        private readonly List<string> _exclusions;

        public WindowsPathSkipper()
        {
            _exclusions = new List<string>()
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    @"$Recycle.Bin",
                    @"\Package Cache",
                    @"\.nuget",
                    @"\Local\Temp",
                };
        }

        public bool ShouldSkip(string path)
        {
            return _exclusions.Any(ex => path.IndexOf(ex, StringComparison.OrdinalIgnoreCase) > -1);
        }
    }
}