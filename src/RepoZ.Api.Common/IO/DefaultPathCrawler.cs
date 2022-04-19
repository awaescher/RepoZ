namespace RepoZ.Api.Common.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DefaultPathCrawler
    {
        private void Find(string root, string searchPattern, List<string> result)
        {
            if (root.IndexOf("$Recycle.Bin", StringComparison.OrdinalIgnoreCase) > -1)
            {
                return;
            }

            foreach (var file in Directory.GetFiles(root, searchPattern))
            {
                result.Add(file);
            }

            foreach (var directory in Directory.GetDirectories(root))
            {
                try
                {
                    Find(directory, searchPattern, result);
                }
                catch
                {
                    // swallow, log, whatever
                }
            }
        }
    }
}
