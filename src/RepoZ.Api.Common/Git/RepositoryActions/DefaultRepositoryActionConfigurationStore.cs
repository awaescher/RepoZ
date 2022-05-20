namespace RepoZ.Api.Common.Git
{
    using Newtonsoft.Json;
    using RepoZ.Api.IO;
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using RepoZ.Api.Git;

    public class DefaultRepositoryActionConfigurationStore : IRepositoryActionConfigurationStore
    {
        private const string REPOSITORY_ACTIONS_FILENAME = "RepositoryActions.json";
        private readonly IErrorHandler _errorHandler;
        private readonly IFileSystem _fileSystem;
        private readonly string _fullFilename;

        public DefaultRepositoryActionConfigurationStore(IErrorHandler errorHandler, IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem)
            // : base(errorHandler, fileSystem)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));

            _fullFilename = Path.Combine(appDataPathProvider.GetAppDataPath(), REPOSITORY_ACTIONS_FILENAME);
        }

        public string GetFileName()
        {
            return _fullFilename;
        }

        public RepositoryActionConfiguration LoadRepositoryConfiguration(Repository repo)
        {
            var file = Path.Combine(repo.Path, ".git", REPOSITORY_ACTIONS_FILENAME);
            if (_fileSystem.File.Exists(file))
            {
                RepositoryActionConfiguration result = LoadRepositoryActionConfiguration(file);
                if (result.State == RepositoryActionConfiguration.LoadState.Ok)
                {
                    return result;
                }
            }

            file = Path.Combine(repo.Path, REPOSITORY_ACTIONS_FILENAME);
            if (_fileSystem.File.Exists(file))
            {
                RepositoryActionConfiguration result = LoadRepositoryActionConfiguration(file);
                if (result.State == RepositoryActionConfiguration.LoadState.Ok)
                {
                    return result;
                }
            }

            return null;
        }

        public RepositoryActionConfiguration LoadGlobalRepositoryActions()
        {
            return LoadRepositoryActionConfiguration(GetFileName());
        }

        public RepositoryActionConfiguration LoadRepositoryActionConfiguration(string filename)
        {
            try
            {
                if (_fileSystem.File.Exists(filename))
                {
                    using Stream stream = _fileSystem.File.OpenRead(filename);
                    return LoadRepositoryActionConfiguration(stream).GetAwaiter().GetResult();
                }

                return LoadRepositoryActionConfigurationFromJson(string.Empty);
            }
            catch (Exception ex)
            {
                _errorHandler.Handle(ex.Message);

                return new RepositoryActionConfiguration
                    {
                        State = RepositoryActionConfiguration.LoadState.Error,
                        LoadError = ex.Message,
                    };
            }
        }

        public async Task<RepositoryActionConfiguration> LoadRepositoryActionConfiguration(Stream stream)
        {
            string jsonContent;
            try
            {
                using var sr = new StreamReader(stream);
                jsonContent = await sr.ReadToEndAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new RepositoryActionConfiguration
                    {
                        State = RepositoryActionConfiguration.LoadState.Error,
                        LoadError = ex.Message,
                    };
            }

            return LoadRepositoryActionConfigurationFromJson(jsonContent);
        }

        public RepositoryActionConfiguration LoadRepositoryActionConfigurationFromJson(string jsonContent)
        {
            try
            {
                var lines = jsonContent.Split(new string[] { Environment.NewLine, }, StringSplitOptions.None);
                var json = string.Join(Environment.NewLine, lines.Select(RemoveComment));
                RepositoryActionConfiguration repositoryActionConfiguration = JsonConvert.DeserializeObject<RepositoryActionConfiguration>(json) ?? new RepositoryActionConfiguration();
                repositoryActionConfiguration.State = RepositoryActionConfiguration.LoadState.Ok;
                return repositoryActionConfiguration;
            }
            catch (Exception ex)
            {
                return new RepositoryActionConfiguration
                    {
                        State = RepositoryActionConfiguration.LoadState.Error,
                        LoadError = ex.Message,
                    };
            }
        }

        private static string RemoveComment(string line)
        {
            var l = line.Trim();
            if (l.Length == 0)
            {
                return line;
            }

            if (l[0] == '#')
            {
                return string.Empty;
            }

            if (l.StartsWith("//"))
            {
                return string.Empty;
            }

            return line;
        }
    }
}