namespace Specs.Mocks
{
    using RepoZ.Api.IO;

    internal class GivenPathProvider : IPathProvider
    {
        public GivenPathProvider(string[] paths)
        {
            Paths = paths;
        }

        public string[] Paths { get; set; }

        public string[] GetPaths()
        {
            return Paths;
        }
    }
}
