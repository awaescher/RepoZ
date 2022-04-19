namespace RepoZ.Api.Common.IO
{
    using RepoZ.Api.IO;

    public class DefaultPathCrawlerFactory : IPathCrawlerFactory
    {
        private readonly IPathSkipper _pathSkipper;

        public DefaultPathCrawlerFactory(IPathSkipper pathSkipper)
        {
            _pathSkipper = pathSkipper;
        }

        public IPathCrawler Create()
        {
            return new GravellPathCrawler(_pathSkipper);
        }
    }
}
