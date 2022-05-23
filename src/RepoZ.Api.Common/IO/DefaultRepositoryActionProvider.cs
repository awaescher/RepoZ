namespace RepoZ.Api.Common.IO
{
    using System.Collections.Generic;
    using RepoZ.Api.Git;
    using System.Linq;
    using System;
    using RepoZ.Api.Common.Common;
    using System.IO.Abstractions;
    using RepoZ.Api.Common.IO.ExpressionEvaluator;
    using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

    public class DefaultRepositoryActionProvider : IRepositoryActionProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly RepositorySpecificConfiguration _repoSpecificConfig;

        public DefaultRepositoryActionProvider(
            IFileSystem fileSystem,
            RepositorySpecificConfiguration repoSpecificConfig)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _repoSpecificConfig = repoSpecificConfig ?? throw new ArgumentNullException(nameof(repoSpecificConfig));
        }

        public RepositoryAction GetPrimaryAction(Repository repository)
        {
            return GetContextMenuActions(new[] { repository, }).FirstOrDefault();
        }

        public RepositoryAction GetSecondaryAction(Repository repository)
        {
            IEnumerable<RepositoryAction> actions = GetContextMenuActions(new[] { repository, }).Take(2);
            return actions.Count() > 1 ? actions.ElementAt(1) : null;
        }

        public IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories)
        {
            return GetContextMenuActionsInternal(repositories.Where(r => _fileSystem.Directory.Exists(r.SafePath))).Where(a => a != null);
        }

        private IEnumerable<RepositoryAction> GetContextMenuActionsInternal(IEnumerable<Repository> repos)
        {
            Repository[] repositories = repos.ToArray();

            try
            {
                return _repoSpecificConfig.CreateActions(repositories);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        // //todo, remove
        // private RepositoryAction CreateProcessRunnerAction(RepositoryActionConfiguration.RepositoryAction action, Repository repository)
        // {
        //     var type = action.Type;
        //     var name = NameHelper.ReplaceTranslatables(NameHelper.ReplaceVariables(_translationService.Translate(action.Name), repository), _translationService);
        //     var command = NameHelper.ReplaceVariables(action.Command, repository);
        //     var executables = action.Executables.Select(e => NameHelper.ReplaceVariables(e, repository));
        //
        //     // var arguments = ReplaceVariables(action.Arguments, repository);
        //     var arguments = _expressionEvaluator.EvaluateStringExpression(action.Arguments, repository);
        //
        //     if ("external commandline provider".Equals(type, StringComparison.CurrentCultureIgnoreCase))
        //     {
        //         return new RepositoryAction()
        //         {
        //             Name = name,
        //             ExecutionCausesSynchronizing = true,
        //             DeferredSubActionsEnumerator = () =>
        //             {
        //                 try
        //                 {
        //                     var repozEnvFile = Path.Combine(repository.Path, ".git", "repoz.env");
        //
        //                     var psi = new ProcessStartInfo(command, arguments)
        //                     {
        //                         WorkingDirectory = new FileInfo(command).DirectoryName,
        //                         CreateNoWindow = true,
        //                         UseShellExecute = false,
        //                         WindowStyle = ProcessWindowStyle.Hidden,
        //                         RedirectStandardOutput = true,
        //                         RedirectStandardError = true,
        //                     };
        //
        //                     if (_fileSystem.File.Exists(repozEnvFile))
        //                     {
        //                         foreach (KeyValuePair<string, string> item in DotNetEnv.Env.Load(repozEnvFile, new DotNetEnv.LoadOptions(setEnvVars: false)))
        //                         {
        //                             psi.EnvironmentVariables.Add(item.Key, item.Value);
        //                         }
        //                     }
        //
        //                     var proc = Process.Start(psi);
        //                     if (proc == null)
        //                     {
        //                         return new RepositoryAction[]
        //                             {
        //                                 new RepositoryAction() { Name = _translationService.Translate("Could not start process"), CanExecute = false, },
        //                             };
        //                     }
        //                     else
        //                     {
        //                         proc.WaitForExit(7500);
        //                         if (proc.HasExited)
        //                         {
        //                             if (proc.ExitCode == 0)
        //                             {
        //                                 var json = proc.StandardOutput.ReadToEnd();
        //
        //                                 RepositoryActionConfiguration actionMenu = _repositoryActionConfigurationStore.LoadRepositoryActionConfigurationFromJson(json);
        //                                 if (actionMenu.State == RepositoryActionConfiguration.LoadState.Error)
        //                                 {
        //                                     return new RepositoryAction[]
        //                                         {
        //                                             new RepositoryAction() { Name = _translationService.Translate("Could not read repository actions"), CanExecute = false, },
        //                                         };
        //                                 }
        //
        //                                 if (actionMenu.RepositoryActions.Count > 0)
        //                                 {
        //                                     return actionMenu.RepositoryActions
        //                                         .Where(x => _expressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
        //                                         .Select(x => CreateProcessRunnerAction(x, repository))
        //                                         .Concat(
        //                                             new RepositoryAction[]
        //                                             {
        //                                                 new RepositoryAction() { Name = $"Last update: {DateTime.Now:HH:mm:ss}", CanExecute = false, },
        //                                             })
        //                                         .ToArray();
        //                                 }
        //                                 else
        //                                 {
        //                                     return new RepositoryAction[]
        //                                     {
        //                                         new RepositoryAction() { Name = $"No entries found.", CanExecute = false, },
        //                                         new RepositoryAction() { Name = $"Last update: {DateTime.Now:HH:mm:ss}", CanExecute = false, },
        //                                     };
        //                                 }
        //                             }
        //                             else
        //                             {
        //                                 var error = proc.StandardError.ReadToEnd();
        //
        //                                 return new RepositoryAction[]
        //                                     {
        //                                         new RepositoryAction() { Name = "No data found.", CanExecute = false, },
        //                                         new RepositoryAction() { Name = $"Exit code {proc.ExitCode}", CanExecute = false, },
        //                                         new RepositoryAction() { Name = string.IsNullOrEmpty(error) ? "Unknown error" : error, CanExecute = false, },
        //                                         new RepositoryAction() { Name = $"Last update: {DateTime.Now:HH:mm:ss}", CanExecute = false, },
        //                                     };
        //                             }
        //                         }
        //                         else
        //                         {
        //                             try
        //                             {
        //                                 proc.Kill();
        //
        //                                 return new RepositoryAction[]
        //                                 {
        //                                     new RepositoryAction() { Name = _translationService.Translate("Could not read repository actions. Process killed successfully"), CanExecute = false, },
        //                                 };
        //                             }
        //                             catch (Exception e)
        //                             {
        //                                 return new RepositoryAction[]
        //                                 {
        //                                     new RepositoryAction() { Name = _translationService.Translate("Could not read repository actions. Process didn't finish. Could not kill process"), CanExecute = false, },
        //                                     new RepositoryAction() { Name = e.Message, CanExecute = false, },
        //                                 };
        //                             }
        //                         }
        //                     }
        //                 }
        //                 catch (Exception e)
        //                 {
        //                     return new RepositoryAction[]
        //                     {
        //                         new RepositoryAction() { Name = _translationService.Translate("Could not read repository actions"), CanExecute = false },
        //                         new RepositoryAction() { Name = e.Message, CanExecute = false }
        //                     };
        //                 }
        //             }
        //         };
        //     }
        //    
        //
        //     return null;
        // }
    }
}