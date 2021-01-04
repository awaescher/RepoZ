using System;

namespace RepoZ.Api.IO
{
	public interface IPathFinder
	{
		bool CanHandle(string processName);

		string FindPath(IntPtr windowHandle);
	}
}