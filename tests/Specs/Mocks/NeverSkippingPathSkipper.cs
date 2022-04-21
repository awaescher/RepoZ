namespace Specs.Mocks
{
    using RepoZ.Api.IO;

    internal class NeverSkippingPathSkipper : IPathSkipper
	{
		public bool ShouldSkip(string path)
        {
            return false;
        }
    }
}
