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

    public class DefaultRepositoryActionProvider : IRepositoryActionProvider
    {
        private readonly IRepositoryActionConfigurationStore _repositoryActionConfigurationStore;
        private readonly IRepositoryWriter _repositoryWriter;
        private readonly IRepositoryMonitor _repositoryMonitor;
        private readonly IErrorHandler _errorHandler;
        private readonly ITranslationService _translationService;
        private readonly RepositoryActionConfiguration _configuration;

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

                    foreach (RepositoryActionConfiguration.RepositoryAction action in config.RepositoryActions.Where(a => RepositoryExpressionEvaluator.EvaluateBooleanExpression(a.Active, singleRepository)))
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

                    foreach (RepositoryActionConfiguration.FileAssociation fileAssociation in config.FileAssociations.Where(a => RepositoryExpressionEvaluator.EvaluateBooleanExpression(a.Active, singleRepository)))
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

            yield return CreateActionForMultipleRepositories(_translationService.Translate("Ignore"), repositories, r => _repositoryMonitor.IgnoreByPath(r.Path), beginGroup: true);
        }


        private static string ReplaceVariables(string value, Repository repository)
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
            var type = action.Type;
            var name = ReplaceTranslatables(ReplaceVariables(_translationService.Translate(action.Name), repository));
            var command = ReplaceVariables(action.Command, repository);
            var executables = action.Executables.Select(e => ReplaceVariables(e, repository));

            // var arguments = ReplaceVariables(action.Arguments, repository);
            var arguments = RepositoryExpressionEvaluator.EvaluateStringExpression(action.Arguments, repository);

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

                            if (File.Exists(repozEnvFile))
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
                                                    new RepositoryAction() { Name = _configuration.LoadError, CanExecute = false, },
                                                };
                                        }

                                        if (actionMenu.RepositoryActions.Count > 0)
                                        {
                                            return actionMenu.RepositoryActions
                                                .Where(x => RepositoryExpressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
                                                .Select(x => CreateProcessRunnerAction(x, repository, false))
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
                                  .Where(x => RepositoryExpressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
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
                return;
            }
            catch (Exception)
            {
                // swallow, retry below.
            }

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo(process, arguments)
                    {
                        UseShellExecute = true,
                    };
                Process.Start(psi);
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