using System.Diagnostics;
using System.Collections.Generic;
using RepoZ.Api.Git;
using System.Linq;
using System;
using RepoZ.Api.Common;

namespace RepoZ.Api.Mac
{
	public class MacRepositoryActionProvider : IRepositoryActionProvider
	{
        private IRepositoryWriter _repositoryWriter;
        private IErrorHandler _errorHandler;

        public MacRepositoryActionProvider(IRepositoryWriter repositoryWriter, IErrorHandler errorHandler)
        {
            _repositoryWriter = repositoryWriter;
            _errorHandler = errorHandler;
        }

        public RepositoryAction GetPrimaryAction(Repository repository)
        { 
            return CreateProcessRunnerAction("Open in Finder", repository.Path);
        }

        public RepositoryAction GetSecondaryAction(Repository repository)
        {
            return CreateProcessRunnerAction("Open in Terminal", "open", $"-b com.apple.terminal \"{repository.Path}\"");
        }

        public IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories)
        {
            var singleRepository = repositories.Count() == 1 ? repositories.Single() : null;

            if (singleRepository != null)
                yield return GetPrimaryAction(singleRepository);
            
            yield return CreateActionForMultipleRepositories("Fetch", repositories, _repositoryWriter.Fetch, beginGroup: true, executionCausesSynchronizing: true);
            yield return CreateActionForMultipleRepositories("Pull", repositories, _repositoryWriter.Pull, executionCausesSynchronizing: true);
            yield return CreateActionForMultipleRepositories("Push", repositories, _repositoryWriter.Push, executionCausesSynchronizing: true);

        }

        private RepositoryAction CreateProcessRunnerAction(string name, string process, string arguments = "")
        {
            return new RepositoryAction()
            {
                Name = name,
                Action = (sender, args) => StartProcess(process, arguments)
            };
        }

        private RepositoryAction CreateActionForMultipleRepositories(string name,
            IEnumerable<Repository> repositories,
            Action<Repository> action,
            bool beginGroup = false,
            bool executionCausesSynchronizing = false)
        {
            return new RepositoryAction()
            {
                Name = name,
                Action = (sender, args) =>
                {
                    // copy over to an array to not get an exception
                    // once the enumerator changes (which can happen when a change
                    // is detected and a repository is renewed) while the loop is running
                    var repositoryArray = repositories.ToArray();

                    foreach (var repository in repositoryArray)
                        SafelyExecute(action, repository); // git/io-exceptions will break the loop, put in try/catch
                },
                BeginGroup = beginGroup,
                ExecutionCausesSynchronizing = executionCausesSynchronizing
            };
        }

        private void SafelyExecute(Action<Repository> action, Repository repository)
        {
            try
            {
                action(repository);
            }
            catch (Exception ex)
            {
                _errorHandler.Handle(ex.Message);
            }
        }

        private void StartProcess(string process, string arguments)
        {
            try
            {
                Process.Start(process, arguments);
            }
            catch (Exception ex)
            {
                _errorHandler.Handle(ex.Message);
            }
        }
	}
}
