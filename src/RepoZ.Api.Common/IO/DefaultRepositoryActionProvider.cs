namespace RepoZ.Api.Common.IO
{
    using System.Diagnostics;
    using System.Collections.Generic;
    using RepoZ.Api.Git;
    using System.Linq;
    using System;
    using RepoZ.Api.Common.Common;
    using System.IO;
    using RepoZ.Api.Common.Git;
    using ExpressionStringEvaluator.Parser;
    using ExpressionStringEvaluator.Methods.BooleanToBoolean;
    using ExpressionStringEvaluator.Methods.Flow;
    using ExpressionStringEvaluator.Methods.StringToBoolean;
    using ExpressionStringEvaluator.Methods.StringToInt;
    using ExpressionStringEvaluator.Methods.StringToString;
    using ExpressionStringEvaluator.Methods;
    using ExpressionStringEvaluator.VariableProviders.DateTime;
    using ExpressionStringEvaluator.VariableProviders;
    using DotNetEnv;

    public class DefaultRepositoryActionProvider : IRepositoryActionProvider
    {
        private readonly IRepositoryActionConfigurationStore _repositoryActionConfigurationStore;
        private readonly IRepositoryWriter _repositoryWriter;
        private readonly IRepositoryMonitor _repositoryMonitor;
        private readonly IErrorHandler _errorHandler;
        private readonly ITranslationService _translationService;
        private readonly RepositoryActionConfiguration _configuration;
        private readonly ExpressionExecutor _expressionExecutor;

        public DefaultRepositoryActionProvider(
            IRepositoryActionConfigurationStore repositoryActionConfigurationStore,
            IRepositoryWriter repositoryWriter,
            IRepositoryMonitor repositoryMonitor,
            IErrorHandler errorHandler,
            ITranslationService translationService)
        {
            _repositoryActionConfigurationStore = repositoryActionConfigurationStore ?? throw new ArgumentNullException(nameof(repositoryActionConfigurationStore));
            _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
            _repositoryMonitor = repositoryMonitor ?? throw new ArgumentNullException(nameof(repositoryMonitor));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));

            _configuration = _repositoryActionConfigurationStore.RepositoryActionConfiguration;

            var dateTimeTimeVariableProviderOptions = new DateTimeVariableProviderOptions()
                {
                    DateTimeProvider = () => DateTime.Now,
                };

            var dateTimeNowVariableProviderOptions = new DateTimeNowVariableProviderOptions()
                {
                    DateTimeProvider = () => DateTime.Now,
                };

            var dateTimeDateVariableProviderOptions = new DateTimeDateVariableProviderOptions()
                {
                    DateTimeProvider = () => DateTime.Now,
                };

            var providers = new List<IVariableProvider>
                {
                    new DateTimeNowVariableProvider(dateTimeNowVariableProviderOptions),
                    new DateTimeTimeVariableProvider(dateTimeTimeVariableProviderOptions),
                    new DateTimeDateVariableProvider(dateTimeDateVariableProviderOptions),
                    new EmptyVariableProvider(),
                    new CustomEnvironmentVariableVariableProvider(GetRepoEnvironmentVariables),
                    new RepositoryVariableProvider(),
                };

            var methods = new List<IMethod>
                {
                    new StringTrimEndStringMethod(),
                    new StringTrimStartStringMethod(),
                    new StringTrimStringMethod(),
                    new StringContainsStringMethod(),
                    new StringLowerStringMethod(),
                    new StringUpperStringMethod(),
                    new UrlEncodeStringMethod(),
                    new UrlDecodeStringMethod(),
                    new StringEqualsStringMethod(),
                    new AndBooleanMethod(),
                    new OrBooleanMethod(),
                    new StringIsNullOrEmptyBooleanMethod(),
                    new FileExistsBooleanMethod(),
                    new NotBooleanMethod(),
                    new StringLengthMethod(),
                    new IfThenElseMethod(),
                    new IfThenMethod(),
                };

            _expressionExecutor = new ExpressionStringEvaluator.Parser.ExpressionExecutor(providers, methods);
        }

        public RepositoryAction GetPrimaryAction(Repository repository)
        {
            return GetContextMenuActions(new[] { repository, }).FirstOrDefault();
        }

        public RepositoryAction GetSecondaryAction(Repository repository)
        {
            IEnumerable<RepositoryAction> actions = GetContextMenuActions(new[] { repository, });
            return actions.Count() > 1 ? actions.ElementAt(1) : null;
        }

        public IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories)
        {
            return GetContextMenuActionsInternal(repositories.Where(r => Directory.Exists(r.SafePath))).Where(a => a != null);
        }

        private IEnumerable<RepositoryAction> GetContextMenuActionsInternal(IEnumerable<Repository> repos)
        {
            Repository[] repositories = repos.ToArray();
            Repository singleRepository = repositories.Count() == 1 ? repositories.Single() : null;

            if (_configuration.State == RepositoryActionConfiguration.LoadState.Error)
            {
                yield return new RepositoryAction()
                    {
                        Name = _translationService.Translate("Could not read repository actions"),
                        CanExecute = false,
                    };
                yield return new RepositoryAction()
                    {
                        Name = _configuration.LoadError,
                        CanExecute = false,
                    };
                var location = ((FileRepositoryStore)_repositoryActionConfigurationStore).GetFileName();
                yield return CreateProcessRunnerAction(_translationService.Translate("Fix"), Path.GetDirectoryName(location));
            }

            // load specific repo config
            RepositoryActionConfiguration specificConfig = null;
            if (singleRepository != null)
            {
                RepositoryActionConfiguration tmpConfig = _repositoryActionConfigurationStore.LoadRepositoryConfiguration(singleRepository);
                specificConfig = tmpConfig;

                if (!string.IsNullOrWhiteSpace(tmpConfig?.RedirectFile))
                {
                    var filename = ReplaceVariables(tmpConfig.RedirectFile, singleRepository);
                    if (File.Exists(filename))
                    {
                        tmpConfig = _repositoryActionConfigurationStore.LoadRepositoryActionConfiguration(filename);

                        if (tmpConfig != null && tmpConfig.State == RepositoryActionConfiguration.LoadState.Ok)
                        {
                            specificConfig = tmpConfig;
                        }
                    }
                }
            }

            if (singleRepository != null && _configuration.State == RepositoryActionConfiguration.LoadState.Ok)
            {
                RepositoryActionConfiguration[] repositoryActionConfigurations = new[] { _configuration, specificConfig, };

                foreach (RepositoryActionConfiguration config in repositoryActionConfigurations)
                {
                    if (config == null || config.State != RepositoryActionConfiguration.LoadState.Ok)
                    {
                        continue;
                    }

                    foreach (var action in config.RepositoryActions.Where(a => EvaluateBooleanExpression(a.Active, singleRepository)))
                    {
                        yield return CreateProcessRunnerAction(action, singleRepository, beginGroup: false);
                    }
                }

                foreach (RepositoryActionConfiguration config in repositoryActionConfigurations)
                {
                    if (config == null || config.State != RepositoryActionConfiguration.LoadState.Ok)
                    {
                        continue;
                    }

                    foreach (RepositoryActionConfiguration.FileAssociation fileAssociation in config.FileAssociations.Where(a => EvaluateBooleanExpression(a.Active, singleRepository)))
                    {
                        yield return CreateFileAssociationSubMenu(
                            singleRepository,
                            ReplaceTranslatables(fileAssociation.Name),
                            fileAssociation.Extension);
                    }
                }

                yield return CreateBrowseRemoteAction(singleRepository);
            }

            yield return CreateActionForMultipleRepositories(_translationService.Translate("Fetch"), repositories, _repositoryWriter.Fetch, beginGroup: true, executionCausesSynchronizing: true);
            yield return CreateActionForMultipleRepositories(_translationService.Translate("Pull"), repositories, _repositoryWriter.Pull, executionCausesSynchronizing: true);
            yield return CreateActionForMultipleRepositories(_translationService.Translate("Push"), repositories, _repositoryWriter.Push, executionCausesSynchronizing: true);

            if (singleRepository != null)
            {
                yield return new RepositoryAction()
                    {
                        Name = _translationService.Translate("Checkout"),
                        DeferredSubActionsEnumerator = () => singleRepository.LocalBranches
                                                                             .Take(50)
                                                                             .Select(branch => new RepositoryAction()
                                                                                 {
                                                                                     Name = branch,
                                                                                     Action = (_, __) => _repositoryWriter.Checkout(singleRepository, branch),
                                                                                     CanExecute = !singleRepository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                                                                 })
                                                                             .Union(new[]
                                                                                 {
                                                                                     new RepositoryAction()
                                                                                         {
                                                                                             BeginGroup = true,
                                                                                             Name = _translationService.Translate("Remote branches"),
                                                                                             DeferredSubActionsEnumerator = () =>
                                                                                                 {
                                                                                                     var remoteBranches = singleRepository.ReadAllBranches().Select(branch => new RepositoryAction()
                                                                                                         {
                                                                                                             Name = branch,
                                                                                                             Action = (_, __) => _repositoryWriter.Checkout(singleRepository, branch),
                                                                                                             CanExecute = !singleRepository.CurrentBranch.Equals(branch,
                                                                                                                 StringComparison.OrdinalIgnoreCase),
                                                                                                         }).ToArray();

                                                                                                     if (remoteBranches.Any())
                                                                                                     {
                                                                                                         return remoteBranches;
                                                                                                     }

                                                                                                     return new RepositoryAction[]
                                                                                                         {
                                                                                                             new RepositoryAction()
                                                                                                                 {
                                                                                                                     Name = _translationService.Translate("No remote branches found"),
                                                                                                                     CanExecute = false,
                                                                                                                 },
                                                                                                             new RepositoryAction()
                                                                                                                 {
                                                                                                                     Name = _translationService.Translate(
                                                                                                                         "Try to fetch changes if you're expecting remote branches"),
                                                                                                                     CanExecute = false,
                                                                                                                 },
                                                                                                         };
                                                                                                 },
                                                                                         },
                                                                                 })
                                                                             .ToArray(),
                    };
            }

            yield return CreateActionForMultipleRepositories(_translationService.Translate("Ignore"), repositories, r => _repositoryMonitor.IgnoreByPath(r.Path), beginGroup: true);
        }

        private bool EvaluateBooleanExpression(string value, Repository repository)
        {
            try
            {
                var result = _expressionExecutor.Execute<Repository>(repository, value);
                if (result.IsBool(out var b))
                {
                    return b.Value;
                }

                if ("true".Equals(result.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Dictionary<string, string> GetRepoEnvironmentVariables(Repository repository)
        {
            var specificRepoEnvValues = new Dictionary<string, string>(0);
            var repozEnvFile = Path.Combine(repository.Path, ".git", "repoz.env");

            if (File.Exists(repozEnvFile))
            {
                try
                {
                    var envVars = DotNetEnv.Env.Load(repozEnvFile, new DotNetEnv.LoadOptions(setEnvVars: false));
                    specificRepoEnvValues = envVars.ToDictionary();
                    return specificRepoEnvValues;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return specificRepoEnvValues;
        }

        private string ReplaceVariables(string value, Repository repository)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return Environment.ExpandEnvironmentVariables(
                value
                    .Replace("{Repository.Name}", repository.Name)
                    .Replace("{Repository.Path}", repository.Path)
                    .Replace("{Repository.SafePath}", repository.SafePath)
                    .Replace("{Repository.Location}", repository.Location)
                    .Replace("{Repository.CurrentBranch}", repository.CurrentBranch)
                    .Replace("{Repository.Branches}", string.Join("|", repository.Branches))
                    .Replace("{Repository.LocalBranches}", string.Join("|", repository.LocalBranches))
                    .Replace("{Repository.RemoteUrls}", string.Join("|", repository.RemoteUrls)));
        }

        private string ReplaceTranslatables(string value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            value = ReplaceTranslatable(value, "Open");
            value = ReplaceTranslatable(value, "OpenIn");
            value = ReplaceTranslatable(value, "OpenWith");

            return value;
        }

        private string ReplaceTranslatable(string value, string translatable)
        {
            if (value.StartsWith("{" + translatable + "}"))
            {
                var rest = value.Replace("{" + translatable + "}", "").Trim();
                return _translationService.Translate("(" + translatable + ")", rest); // XMl doesn't support {}
            }

            return value;
        }

        private RepositoryAction CreateProcessRunnerAction(RepositoryActionConfiguration.RepositoryAction action, Repository repository, bool beginGroup = false)
        {
            var name = ReplaceTranslatables(ReplaceVariables(_translationService.Translate(action.Name), repository));
            var command = ReplaceVariables(action.Command, repository);
            var executables = action.Executables.Select(e => ReplaceVariables(e, repository));
            var arguments = ReplaceVariables(action.Arguments, repository);

            if (action.Subfolder.Any())
            {
                return new RepositoryAction()
                    {
                        Name = name,
                        DeferredSubActionsEnumerator = () =>
                            action.Subfolder
                                  .Select(x => CreateProcessRunnerAction(x, repository, false))
                                  .ToArray(),
                    };
            }

            if (command.Equals("browser", StringComparison.CurrentCultureIgnoreCase))
            {
                // assume arguments is an url.
                return CreateProcessRunnerAction(name, arguments);
            }
            else if (string.IsNullOrEmpty(action.Command))
            {
                foreach (var executable in executables)
                {
                    var normalized = executable.Replace("\"", "");
                    if (File.Exists(normalized) || Directory.Exists(normalized))
                    {
                        return new RepositoryAction()
                            {
                                BeginGroup = beginGroup,
                                Name = name,
                                Action = (_, __) => StartProcess(executable, arguments),
                            };
                    }
                }
            }
            else
            {
                return new RepositoryAction()
                    {
                        BeginGroup = beginGroup,
                        Name = name,
                        Action = (_, __) => StartProcess(command, arguments),
                    };
            }

            return null;
        }

        private RepositoryAction CreateProcessRunnerAction(string name, string process, string arguments = "", bool beginGroup = false)
        {
            return new RepositoryAction()
                {
                    BeginGroup = beginGroup,
                    Name = name,
                    Action = (_, __) => StartProcess(process, arguments),
                };
        }

        private RepositoryAction CreateActionForMultipleRepositories(
            string name,
            IEnumerable<Repository> repositories,
            Action<Repository> action,
            bool beginGroup = false,
            bool executionCausesSynchronizing = false)
        {
            return new RepositoryAction()
                {
                    Name = name,
                    Action = (_, __) =>
                        {
                            // copy over to an array to not get an exception
                            // once the enumerator changes (which can happen when a change
                            // is detected and a repository is renewed) while the loop is running
                            Repository[] repositoryArray = repositories.ToArray();

                            foreach (Repository repository in repositoryArray)
                            {
                                SafelyExecute(action, repository); // git/io-exceptions will break the loop, put in try/catch
                            }
                        },
                    BeginGroup = beginGroup,
                    ExecutionCausesSynchronizing = executionCausesSynchronizing,
                };
        }

        private void SafelyExecute(Action<Repository> action, Repository repository)
        {
            try
            {
                action(repository);
            }
            catch
            {
                // nothing to see here
            }
        }

        private void StartProcess(string process, string arguments)
        {
            try
            {
                Debug.WriteLine("Starting: " + process + arguments);
                Process.Start(process, arguments);
            }
            catch (Exception ex)
            {
                _errorHandler.Handle(ex.Message);
            }
        }

        private RepositoryAction CreateFileAssociationSubMenu(Repository repository, string actionName, string filePattern)
        {
            if (HasFiles(repository, filePattern))
            {
                return new RepositoryAction()
                    {
                        Name = actionName,
                        DeferredSubActionsEnumerator = () =>
                            GetFiles(repository, filePattern)
                                .Select(solutionFile => ReplaceVariables(solutionFile, repository))
                                .Select(solutionFile => CreateProcessRunnerAction(Path.GetFileName(solutionFile), solutionFile))
                                .ToArray(),
                    };
            }

            return null;
        }

        private RepositoryAction CreateBrowseRemoteAction(Repository repository)
        {
            if (repository.RemoteUrls.Length == 0)
            {
                return null;
            }

            var actionName = _translationService.Translate("Browse remote");

            if (repository.RemoteUrls.Length == 1)
            {
                return CreateProcessRunnerAction(actionName, repository.RemoteUrls[0]);
            }

            return new RepositoryAction()
                {
                    Name = actionName,
                    DeferredSubActionsEnumerator = () => repository.RemoteUrls
                                                                   .Take(50)
                                                                   .Select(url => CreateProcessRunnerAction(url, url))
                                                                   .ToArray(),
                };
        }

        private bool HasFiles(Repository repository, string searchPattern)
        {
            return GetFileEnumerator(repository, searchPattern).Any();
        }

        private IEnumerable<string> GetFiles(Repository repository, string searchPattern)
        {
            return GetFileEnumerator(repository, searchPattern)
                   .Take(25)
                   .OrderBy(f => f);
        }

        private static IEnumerable<string> GetFileEnumerator(Repository repository, string searchPattern)
        {
            // prefer EnumerateFileSystemInfos() over EnumerateFiles() to include packaged folders like
            // .app or .xcodeproj on macOS

            var directory = new DirectoryInfo(repository.Path);
            return directory
                   .EnumerateFileSystemInfos(searchPattern, SearchOption.AllDirectories)
                   .Select(f => f.FullName)
                   .Where(f => !f.StartsWith("."));
        }
    }
}