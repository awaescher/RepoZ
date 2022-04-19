namespace RepoZ.Api.IO
{
    using System;

    public interface IPathFinder
    {
        bool CanHandle(string processName);

        string FindPath(IntPtr windowHandle);
    }
}