namespace RepoZ.Api.IO
{
    using System;
    using System.Collections.Generic;

    public interface IPathCrawler
    {
        List<string> Find(string root, string searchPattern, Action<string> onFoundAction, Action onQuit);
    }
}