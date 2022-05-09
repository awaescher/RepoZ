namespace RepoZ.Api.Common.IO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RepoZ.Api.Common.Common;
    using RepoZ.Api.IO;

    public class GitRepositoryFinderFactory : IGitRepositoryFinderFactory
    {
        private readonly IAppSettingsService _appSettingsService;
        private readonly List<ISingleGitRepositoryFinderFactory> _factories;

        public GitRepositoryFinderFactory(IAppSettingsService appSettingsService, IEnumerable<ISingleGitRepositoryFinderFactory> factories)
        {
            _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
            _factories = factories?.ToList() ?? throw new ArgumentNullException(nameof(factories));
        }

        public IGitRepositoryFinder Create()
        {
            ISingleGitRepositoryFinderFactory factory = null;

            foreach (var enabledSearchProviderName in _appSettingsService.EnabledSearchProviders)
            {
                if (!string.IsNullOrWhiteSpace(enabledSearchProviderName))
                {
                    factory = _factories.FirstOrDefault(searchProviderFactory => searchProviderFactory.IsActive
                                                                                 &&
                                                                                 searchProviderFactory.Name.Equals(enabledSearchProviderName, StringComparison.CurrentCultureIgnoreCase));
                }

                if (factory != null)
                {
                    return factory.Create();
                }
            }

            // Default, fallback
            factory = _factories.FirstOrDefault(searchProviderFactory => searchProviderFactory is GravellGitRepositoryFinderFactory);
            if (factory != null)
            {
                return factory.Create();
            }

            throw new Exception("Could not create IGitRepositoryFinder");
        }
    }
}
