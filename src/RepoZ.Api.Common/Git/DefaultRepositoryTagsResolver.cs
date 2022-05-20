namespace RepoZ.Api.Common.Git
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using RepoZ.Api.Common.IO;
    using RepoZ.Api.Common.IO.ExpressionEvaluator;
    using RepoZ.Api.Git;

    public class DefaultRepositoryTagsResolver : IRepositoryTagsResolver
    {
        private readonly IRepositoryActionConfigurationStore _configStore;
        private readonly IFileSystem _fileSystem;
        private readonly RepositoryExpressionEvaluator _expressionEvaluator;

        public DefaultRepositoryTagsResolver(IRepositoryActionConfigurationStore configStore, IFileSystem fileSystem, RepositoryExpressionEvaluator expressionEvaluator)
        {
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _expressionEvaluator = expressionEvaluator;
            _configStore.LoadGlobalRepositoryActions();
        }

        public void UpdateTags(Repository repository)
        {
            RepositoryActionConfiguration globalConfig = _configStore.LoadGlobalRepositoryActions();
            if (globalConfig == null)
            {
                return;
            }

            if (globalConfig?.State != RepositoryActionConfiguration.LoadState.Ok)
            {
                repository.Tags = Array.Empty<string>();
            }

            var globalConfigTags = globalConfig.RepositoryTags?
                                               .Where(y => _expressionEvaluator.EvaluateBooleanExpression(y.Select, repository))
                                               .Select(x => x.Tag)
                                               .ToArray()
                                   ?? Array.Empty<string>();

            var repositoryConfigTags = Array.Empty<string>();

            RepositoryActionConfiguration repoConfig = _configStore.LoadRepositoryConfiguration(repository);
            if (repoConfig != null && repoConfig.State == RepositoryActionConfiguration.LoadState.Ok)
            {
                if (!string.IsNullOrWhiteSpace(repoConfig.RedirectFile))
                {
                    if (_fileSystem.File.Exists(repoConfig.RedirectFile))
                    {
                        try
                        {
                            RepositoryActionConfiguration redirectConfig = _configStore.LoadRepositoryActionConfiguration(repoConfig.RedirectFile);
                            if (redirectConfig != null && redirectConfig.State == RepositoryActionConfiguration.LoadState.Ok)
                            {
                                repoConfig = redirectConfig;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            if (repoConfig != null && repoConfig.State == RepositoryActionConfiguration.LoadState.Ok)
            {
                repositoryConfigTags = repoConfig.RepositoryTags
                                                 .Where(y => _expressionEvaluator.EvaluateBooleanExpression(y.Select, repository))
                                                 .Select(x => x.Tag)
                                                 .ToArray();
            }

            repository.Tags = globalConfigTags.Concat(repositoryConfigTags).Distinct().ToArray();
        }
    }
}