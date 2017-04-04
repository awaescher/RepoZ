using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.Api.IO
{
    public interface IPathFinder
    {
        bool CanHandle(string processName);

        string FindPath(IntPtr windowHandle);
    }
}