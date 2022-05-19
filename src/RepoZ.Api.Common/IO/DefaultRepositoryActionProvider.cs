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
    using System.IO.Abstractions;
    using RepoZ.Api.Common.IO.ExpressionEvaluator;
    using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
    using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

    public class DefaultRepositoryActionProvider : IRepositoryActionProvider
    {
        private readonly IRepositoryActionConfigurationStore _repositoryActionConfigurationStore;
        private readonly IRepositoryWriter _repositoryWriter;
        private readonly IRepositoryMonitor _repositoryMonitor;
        private readonly IErrorHandler _errorHandler;
        private readonly ITranslationService _translationService;
        private readonly IFileSystem _fileSystem;
        private readonly RepositoryExpressionEvaluator _expressionEvaluator;
        private readonly RepositorySpecificConfiguration _repoSpecificConfig;

        public DefaultRepositoryActionProvider(
            IRepositoryActionConfigurationStore repositoryActionConfigurationStore,
            IRepositoryWriter repositoryWriter,
            IRepositoryMonitor repositoryMonitor,
            IErrorHandler errorHandler,
            ITranslationService translationService,
            IFileSystem fileSystem,
            RepositoryExpressionEvaluator expressionEvaluator,
            RepositorySpecificConfiguration repoSpecificConfig)
        {
            _repositoryActionConfigurationStore = repositoryActionConfigurationStore ?? throw new ArgumentNullException(nameof(repositoryActionConfigurationStore));
            _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
            _repositoryMonitor = repositoryMonitor ?? throw new ArgumentNullException(nameof(repositoryMonitor));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
            _repoSpecificConfig = repoSpecificConfig ?? throw new ArgumentNullException(nameof(repoSpecificConfig));
        }

        private RepositoryActionConfiguration Configuration => _repositoryActionConfigurationStore.RepositoryActionConfiguration;

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
            return GetContextMenuActionsInternal(repositories.Where(r => _fileSystem.Directory.Exists(r.SafePath))).Where(a => a != null);
        }

        private IEnumerable<RepositoryAction> GetContextMenuActionsInternal(IEnumerable<Repository> repos)
        {
            Repository[] repositories = repos.ToArray();
            
            foreach (RepositoryAction x in _repoSpecificConfig.Create(repositories))
            {
                yield return x;
            }
        }

        private IEnumerable<RepositoryAction> Ge1tContextMenuActionsInternal(IEnumerable<Repository> repos)
        {
            Repository[] repositories = repos.ToArray();
            Repository singleRepository = repositories.Count() == 1 ? repositories.Single() : null;

            if (Configuration.State == RepositoryActionConfiguration.LoadState.Error)
            {
                yield return new RepositoryAction()
                    {
                        Name = _translationService.Translate("Could not read repository actions"),
                        CanExecute = false,
                    };
                yield return new RepositoryAction()
                    {
                        Name = Configuration.LoadError,
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
                    var filename = NameHelper.ReplaceVariables(tmpConfig.RedirectFile, singleRepository);
                    if (_fileSystem.File.Exists(filename))
                    {
                        tmpConfig = _repositoryActionConfigurationStore.LoadRepositoryActionConfiguration(filename);

                        if (tmpConfig != null && tmpConfig.State == RepositoryActionConfiguration.LoadState.Ok)
                        {
                            specificConfig = tmpConfig;
                        }
                    }
                }
            }

            if (singleRepository != null && Configuration.State == RepositoryActionConfiguration.LoadState.Ok)
            {
                RepositoryActionConfiguration[] repositoryActionConfigurations = new[] { Configuration, specificConfig, };

                foreach (RepositoryActionConfiguration config in repositoryActionConfigurations)
                {
                    if (config == null || config.State != RepositoryActionConfiguration.LoadState.Ok)
                    {
                        continue;
                    }

                    foreach (RepositoryActionConfiguration.RepositoryAction action in config.RepositoryActions.Where(a => _expressionEvaluator.EvaluateBooleanExpression(a.Active, singleRepository)))
                    {
                        yield return CreateProcessRunnerAction(action, singleRepository);
                    }
                }

                foreach (RepositoryActionConfiguration config in repositoryActionConfigurations)
                {
                    if (config == null || config.State != RepositoryActionConfiguration.LoadState.Ok)
                    {
                        continue;
                    }

                    // foreach (RepositoryActionConfiguration.FileAssociation fileAssociation in config.FileAssociations.Where(a => _expressionEvaluator.EvaluateBooleanExpression(a.Active, singleRepository)))
                    // {
                    //     yield return CreateFileAssociationSubMenu(
                    //         singleRepository,
                    //         NameHelper.ReplaceTranslatables(fileAssociation.Name, _translationService),
                    //         fileAssociation.Extension);
                    // }
                }

                yield return new ActionBrowseRepositoryV1Mapper(_expressionEvaluator, _translationService, _errorHandler).CreateBrowseRemoteAction(singleRepository);
            }

            // yield return new RepositorySeparatorAction();
            // yield return MultipleRepositoryActionHelper.CreateActionForMultipleRepositories(_translationService.Translate("Fetch"), repositories, _repositoryWriter.Fetch, executionCausesSynchronizing: true);
            // yield return MultipleRepositoryActionHelper.CreateActionForMultipleRepositories(_translationService.Translate("Pull"), repositories, _repositoryWriter.Pull, executionCausesSynchronizing: true);
            // yield return MultipleRepositoryActionHelper.CreateActionForMultipleRepositories(_translationService.Translate("Push"), repositories, _repositoryWriter.Push, executionCausesSynchronizing: true);

            if (singleRepository != null)
            {
                // moved
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
                                                                                     new RepositorySeparatorAction(),
                                                                                     new RepositoryAction()
                                                                                         {
                                                                                             Name = _translationService.Translate("Remote branches"),
                                                                                             DeferredSubActionsEnumerator = () =>
                                                                                                 {
                                                                                                     RepositoryAction[] remoteBranches = singleRepository.ReadAllBranches()
                                                                                                         .Select(branch => new RepositoryAction()
                                                                                                             {
                                                                                                                 Name = branch,
                                                                                                                 Action = (_, __) => _repositoryWriter.Checkout(singleRepository, branch),
                                                                                                                 CanExecute = !singleRepository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                                                                                             })
                                                                                                         .ToArray();

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
                                                                                                                     Name = _translationService.Translate("Try to fetch changes if you're expecting remote branches"),
                                                                                                                     CanExecute = false,
                                                                                                                 },
                                                                                                         };
                                                                                                 },
                                                                                         },
                                                                                 })
                                                                             .ToArray(),
                    };
            }

            // yield return new RepositorySeparatorAction();
            // yield return MultipleRepositoryActionHelper.CreateActionForMultipleRepositories(_translationService.Translate("Ignore"), repositories, r => _repositoryMonitor.IgnoreByPath(r.Path));
        }

        private RepositoryAction CreateProcessRunnerAction(RepositoryActionConfiguration.RepositoryAction action, Repository repository)
        {
            var type = action.Type;
            var name = NameHelper.ReplaceTranslatables(NameHelper.ReplaceVariables(_translationService.Translate(action.Name), repository), _translationService);
            var command = NameHelper.ReplaceVariables(action.Command, repository);
            var executables = action.Executables.Select(e => NameHelper.ReplaceVariables(e, repository));

            // var arguments = ReplaceVariables(action.Arguments, repository);
            var arguments = _expressionEvaluator.EvaluateStringExpression(action.Arguments, repository);

            if ("external commandline provider".Equals(type, StringComparison.CurrentCultureIgnoreCase))
            {
                return new RepositoryAction()
                {
                    Name = name,
                    ExecutionCausesSynchronizing = true,
                    DeferredSubActionsEnumerator = () =>
                    {
                        try
                        {
                            var repozEnvFile = Path.Combine(repository.Path, ".git", "repoz.env");

                            var psi = new ProcessStartInfo(command, arguments)
                            {
                                WorkingDirectory = new FileInfo(command).DirectoryName,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                            };

                            if (_fileSystem.File.Exists(repozEnvFile))
                            {
                                foreach (KeyValuePair<string, string> item in DotNetEnv.Env.Load(repozEnvFile, new DotNetEnv.LoadOptions(setEnvVars: false)))
                                {
                                    psi.EnvironmentVariables.Add(item.Key, item.Value);
                                }
                            }

                            var proc = Process.Start(psi);
                            if (proc == null)
                            {
                                return new RepositoryAction[]
                                    {
                                        new RepositoryAction() { Name = _translationService.Translate("Could not start process"), CanExecute = false, },
                                    };
                            }
                            else
                            {
                                proc.WaitForExit(7500);
                                if (proc.HasExited)
                                {
                                    if (proc.ExitCode == 0)
                                    {
                                        var json = proc.StandardOutput.ReadToEnd();

                                        RepositoryActionConfiguration actionMenu = _repositoryActionConfigurationStore.LoadRepositoryActionConfigurationFromJson(json);
                                        if (actionMenu.State == RepositoryActionConfiguration.LoadState.Error)
                                        {
                                            return new RepositoryAction[]
                                                {
                                                    new RepositoryAction() { Name = _translationService.Translate("Could not read repository actions"), CanExecute = false, },
                                                    new RepositoryAction() { Name = Configuration.LoadError, CanExecute = false, },
                                                };
                                        }

                                        if (actionMenu.RepositoryActions.Count > 0)
                                        {
                                            return actionMenu.RepositoryActions
                                                .Where(x => _expressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
                                                .Select(x => CreateProcessRunnerAction(x, repository))
                                                .Concat(
                                                    new RepositoryAction[]
                                                    {
                                                        new RepositoryAction() { Name = $"Last update: {DateTime.Now:HH:mm:ss}", CanExecute = false, },
                                                    })
                                                .ToArray();
                                        }
                                        else
                                        {
                                            return new RepositoryAction[]
                                            {
                                                new RepositoryAction() { Name = $"No entries found.", CanExecute = false, },
                                                new RepositoryAction() { Name = $"Last update: {DateTime.Now:HH:mm:ss}", CanExecute = false, },
                                            };
                                        }
                                    }
                                    else
                                    {
                                        var error = proc.StandardError.ReadToEnd();

                                        return new RepositoryAction[]
                                            {
                                                new RepositoryAction() { Name = "No data found.", CanExecute = false, },
                                                new RepositoryAction() { Name = $"Exit code {proc.ExitCode}", CanExecute = false, },
                                                new RepositoryAction() { Name = string.IsNullOrEmpty(error) ? "Unknown error" : error, CanExecute = false, },
                                                new RepositoryAction() { Name = $"Last update: {DateTime.Now:HH:mm:ss}", CanExecute = false, },
                                            };
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        proc.Kill();

                                        return new RepositoryAction[]
                                        {
                                            new RepositoryAction() { Name = _translationService.Translate("Could not read repository actions. Process killed successfully"), CanExecute = false, },
                                        };
                                    }
                                    catch (Exception e)
                                    {
                                        return new RepositoryAction[]
                                        {
                                            new RepositoryAction() { Name = _translationService.Translate("Could not read repository actions. Process didn't finish. Could not kill process"), CanExecute = false, },
                                            new RepositoryAction() { Name = e.Message, CanExecute = false, },
                                        };
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            return new RepositoryAction[]
                            {
                                new RepositoryAction() { Name = _translationService.Translate("Could not read repository actions"), CanExecute = false },
                                new RepositoryAction() { Name = e.Message, CanExecute = false }
                            };
                        }
                    }
                };
            }

            if (action.Subfolder.Any())
            {
                return new RepositoryAction()
                    {
                        Name = name,
                        DeferredSubActionsEnumerator = () =>
                            action.Subfolder
                                  .Where(x => _expressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
                                  .Select(x => CreateProcessRunnerAction(x, repository))
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
                    if (_fileSystem.File.Exists(normalized) || _fileSystem.Directory.Exists(normalized))
                    {
                        return new RepositoryAction()
                            {
                                Name = name,
                                Action = (_, _) => ProcessHelper.StartProcess(executable, arguments, _errorHandler),
                            };
                    }
                }
            }
            else
            {
                return new RepositoryAction()
                    {
                        Name = name,
                        Action = (_, _) => ProcessHelper.StartProcess(command, arguments, _errorHandler),
                    };
            }

            return null;
        }

        private RepositoryAction CreateProcessRunnerAction(string name, string process, string arguments = "")
        {
            return new RepositoryAction()
                {
                    Name = name,
                    Action = (_, __) => ProcessHelper.StartProcess(process, arguments, _errorHandler),
                };
        }
    }

    public static class NameHelper
    {
        public static string EvaluateName(in string input, in Repository repository, ITranslationService translationService, RepositoryExpressionEvaluator repositoryExpressionEvaluator)
        {
            return repositoryExpressionEvaluator.EvaluateStringExpression(
                ReplaceTranslatables(
                    ReplaceVariables(
                        translationService.Translate(input),
                        repository),
                    translationService),
                repository);
        }

        public static string ReplaceVariables(string value, Repository repository)
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
                    .Replace("{Repository.Branches}", string.Join("|", repository.Branches ?? Array.Empty<string>()))
                    .Replace("{Repository.LocalBranches}", string.Join("|", repository.LocalBranches ?? Array.Empty<string>()))
                    .Replace("{Repository.RemoteUrls}", string.Join("|", repository.RemoteUrls ?? Array.Empty<string>())));
        }

        public static string ReplaceTranslatables(string value, ITranslationService translationService)
        {
            if (value is null)
            {
                return string.Empty;
            }

            value = ReplaceTranslatable(value, "Open", translationService);
            value = ReplaceTranslatable(value, "OpenIn", translationService);
            value = ReplaceTranslatable(value, "OpenWith", translationService);

            return value;
        }

        public static string ReplaceTranslatable(string value, string translatable, ITranslationService translationService)
        {
            if (value.StartsWith("{" + translatable + "}"))
            {
                var rest = value.Replace("{" + translatable + "}", "").Trim();
                return translationService.Translate("(" + translatable + ")", rest); // XMl doesn't support {}
            }

            return value;
        }
    }

    public static class MultipleRepositoryActionHelper
    {
        public static RepositoryAction CreateActionForMultipleRepositories(
            string name,
            IEnumerable<Repository> repositories,
            Action<Repository> action,
            bool executionCausesSynchronizing = false)
        {
            return new RepositoryAction()
                {
                    Name = name,
                    Action = (_, _) =>
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
                    ExecutionCausesSynchronizing = executionCausesSynchronizing,
                };
        }

        private static void SafelyExecute(Action<Repository> action, Repository repository)
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
    }
}