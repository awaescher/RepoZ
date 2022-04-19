using RepoZ.Api.IO;

namespace Specs.Mocks
{
	internal class GivenPathProvider : IPathProvider
	{
		public GivenPathProvider(string[] paths)
		{
			Paths = paths;
		}

		public string[] Paths { get; set; }

		public string[] GetPaths() => Paths;
	}
}
