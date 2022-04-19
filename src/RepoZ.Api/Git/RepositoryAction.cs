namespace RepoZ.Api.Git
{
    using System;

    public class RepositoryAction
    {
        public string Name { get; set; }

        public Action<object, object> Action { get; set; }

        public bool BeginGroup { get; set; }

        public bool ExecutionCausesSynchronizing { get; set; }

        public bool CanExecute { get; set; } = true;

        public Func<RepositoryAction[]> DeferredSubActionsEnumerator { get; set; }
    }
}