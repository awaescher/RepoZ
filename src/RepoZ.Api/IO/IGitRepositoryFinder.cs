namespace RepoZ.Api.IO
{
    using System;
    using System.Collections.Generic;

    public interface IGitRepositoryFinder
    {
        List<string> Find(string root, Action<string> onFoundAction);
    }
}