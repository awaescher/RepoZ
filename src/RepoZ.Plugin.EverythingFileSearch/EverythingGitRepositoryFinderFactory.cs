namespace RepoZ.Plugin.EverythingFileSearch
{
    using System;
    using RepoZ.Api.IO;
    using RepoZ.Plugin.EverythingFileSearch.Internal;

    public class EverythingGitRepositoryFinderFactory : ISingleGitRepositoryFinderFactory
    {
        private readonly IPathSkipper _pathSkipper;

        public EverythingGitRepositoryFinderFactory(IPathSkipper pathSkipper)
        {
            _pathSkipper = pathSkipper ?? throw new ArgumentNullException(nameof(pathSkipper));
        }

        public string Name { get; } = "Everything";

        public bool IsActive { get; } = Everything64Api.IsInstalled();

        public IGitRepositoryFinder Create()
        {
            return new EverythingGitRepositoryFinder(_pathSkipper);
        }
    }
}