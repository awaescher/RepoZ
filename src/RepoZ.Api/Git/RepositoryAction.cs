namespace RepoZ.Api.Git
{
    using System;
    using System.Collections.Generic;

    public class RepositorySeparatorAction : RepositoryAction/*, RepositoryActionBase*/
    {
    }

    public class RepositoryAction : RepositoryActionBase
    {
        public string Name { get; set; }

        public Action<object, object> Action { get; set; }

        public bool ExecutionCausesSynchronizing { get; set; }

        public bool CanExecute { get; set; } = true;

        public Func<RepositoryAction[]> DeferredSubActionsEnumerator { get; set; }

        public IEnumerable<RepositoryAction> SubActions { get; set; }
    }

    public abstract class RepositoryActionBase
    {
    }
}