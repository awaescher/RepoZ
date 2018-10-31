using AppKit;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Git;
using RepoZ.Api.Common.IO;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepoZ.Api.Mac;
using RepoZ.Api.Mac.Git;
using RepoZ.Api.Mac.IO;
using TinyIoC;

namespace RepoZ.App.Mac
{
	static class MainClass
	{
		static void Main(string[] args)
		{
            NSApplication.Init();
            NSApplication.Main(args);
		}
	}
}
