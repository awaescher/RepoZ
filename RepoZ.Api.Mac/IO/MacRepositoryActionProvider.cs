using System.Diagnostics;
using System.Collections.Generic;
using RepoZ.Api.Git;
using System.Linq;
using System;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Common;

namespace RepoZ.Api.Mac
{
	public class MacRepositoryActionProvider : IRepositoryActionProvider
	{
        private readonly IRepositoryWriter _repositoryWriter;
        private readonly IRepositoryMonitor _repositoryMonitor;
        private readonly IErrorHandler _errorHandler;
		private readonly ITranslationService _translationService;

		public MacRepositoryActionProvider(
            IRepositoryWriter repositoryWriter,
            IRepositoryMonitor repositoryMonitor,
            IErrorHandler errorHandler,
			ITranslationService translationService)
        {
            _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
            _repositoryMonitor = repositoryMonitor ?? throw new ArgumentNullException(nameof(repositoryMonitor));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
			_translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
		}

        public RepositoryAction GetPrimaryAction(Repository repository)
        { 
            return CreateProcessRunnerAction(_translationService.Translate("Open in Finder"), repository.Path);
        }

        public RepositoryAction GetSecondaryAction(Repository repository)
        {
            return CreateProcessRunnerAction(_translationService.Translate("Open in Terminal"), "open", $"-b com.apple.terminal \"{repository.Path}\"");
        }

        public IEnumerable<RepositoryAction> GetContextMenuActions(IEnumerable<Repository> repositories)
        {
            var singleRepository = repositories.Count() == 1 ? repositories.Single() : null;

            if (singleRepository != null)
            {
                yield return GetPrimaryAction(singleRepository);
                yield return GetSecondaryAction(singleRepository);
            }
            
            yield return CreateActionForMultipleRepositories(_translationService.Translate("Fetch"), repositories, _repositoryWriter.Fetch, beginGroup: true, executionCausesSynchronizing: true);
            yield return CreateActionForMultipleRepositories(_translationService.Translate("Pull"), repositories, _repositoryWriter.Pull, executionCausesSynchronizing: true);
            yield return CreateActionForMultipleRepositories(_translationService.Translate("Push"), repositories, _repositoryWriter.Push, executionCausesSynchronizing: true);

            yield return CreateActionForMultipleRepositories(_translationService.Translate("Ignore"), repositories, r => _repositoryMonitor.IgnoreByPath(r.Path), beginGroup: true, executionCausesSynchronizing: true);
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
