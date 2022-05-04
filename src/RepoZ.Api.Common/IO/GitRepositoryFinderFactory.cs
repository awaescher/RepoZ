namespace RepoZ.Api.Common.IO
{
    using System;
    using RepoZ.Api.Common.Common;
    using RepoZ.Api.Common.IO.VoidToolsEverything;
    using RepoZ.Api.IO;

    public class GitRepositoryFinderFactory : IGitRepositoryFinderFactory
    {
        private readonly IPathSkipper _pathSkipper;
        private readonly IAppSettingsService _appSettingsService;
        private bool? _isEverythingInstalled;
        private readonly object _lock = new object();

        public GitRepositoryFinderFactory(IPathSkipper pathSkipper, IAppSettingsService appSettingsService)
        {
            _pathSkipper = pathSkipper ?? throw new ArgumentNullException(nameof(pathSkipper));
            _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        }

        public IGitRepositoryFinder Create()
        {
            if (UseEverything())
            {
                return new EverythingGitRepositoryFinder(_pathSkipper);
            }
            else
            {
                return new GravellGitRepositoryFinder(_pathSkipper);
            }
        }

        private bool UseEverything()
        {
            if (!_appSettingsService.EnabledSearchRepoEverything)
            {
                return false;
            }

            if (_isEverythingInstalled.HasValue)
            {
                return _isEverythingInstalled.Value;
            }

            lock (_lock)
            {
                if (!_isEverythingInstalled.HasValue)
                {
                    _isEverythingInstalled = Everything64Api.IsInstalled();
                }
            }

            return _isEverythingInstalled.Value;
        }
    }
}
