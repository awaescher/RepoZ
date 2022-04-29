namespace RepoZ.Api.Common.Git
{
    using Newtonsoft.Json;
    using RepoZ.Api.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using RepoZ.Api.Git;

    public class DefaultRepositoryActionConfigurationStore : FileRepositoryStore, IRepositoryActionConfigurationStore
    {
        private const string REPOSITORY_ACTIONS_FILENAME = "RepositoryActions.json";
        private readonly object _lock = new object();
        private readonly IErrorHandler _errorHandler;
        private readonly string _fullFilename;

        public DefaultRepositoryActionConfigurationStore(IErrorHandler errorHandler, IAppDataPathProvider appDataPathProvider)
            : base(errorHandler)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _ = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));

            _fullFilename = Path.Combine(appDataPathProvider.GetAppDataPath(), REPOSITORY_ACTIONS_FILENAME);
        }

        public override string GetFileName()
        {
            return _fullFilename;
        }

        public RepositoryActionConfiguration LoadRepositoryConfiguration(Repository repo)
        {
            var file = Path.Combine(repo.Path, ".git", REPOSITORY_ACTIONS_FILENAME);
            if (File.Exists(file))
            {
                RepositoryActionConfiguration result = LoadRepositoryActionConfiguration(file);
                if (result.State == RepositoryActionConfiguration.LoadState.Ok)
                {
                    return result;
                }
            }

            file = Path.Combine(repo.Path, REPOSITORY_ACTIONS_FILENAME);
            if (File.Exists(file))
            {
                RepositoryActionConfiguration result = LoadRepositoryActionConfiguration(file);
                if (result.State == RepositoryActionConfiguration.LoadState.Ok)
                {
                    return result;
                }
            }

            return null;
        }

        public void Preload()
        {
            lock (_lock)
            {
                if (!File.Exists(GetFileName()))
                {
                    if (!TryCopyDefaultJsonFile())
                    {
                        RepositoryActionConfiguration = new RepositoryActionConfiguration
                            {
                                State = RepositoryActionConfiguration.LoadState.None,
                            };
                        return;
                    }
                }

                RepositoryActionConfiguration = LoadRepositoryActionConfiguration(GetFileName());
            }
        }

        public RepositoryActionConfiguration LoadRepositoryActionConfiguration(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    using FileStream stream = File.OpenRead(filename);
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

        private bool TryCopyDefaultJsonFile()
        {
            var defaultFile = Path.Combine(_fullFilename);
            var targetFile = GetFileName();

            try
            {
                File.Copy(defaultFile, targetFile);
            }
            catch
            {
                /* lets ignore errors here, we just want to know if if worked or not by checking the file existence */
            }

            return File.Exists(targetFile);
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

        public RepositoryActionConfiguration RepositoryActionConfiguration { get; private set; }
    }
}