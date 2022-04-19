namespace RepoZ.Api.Git
{
    using System;

    public interface IRepositoryObserver : IDisposable
    {
        void Setup(Repository repository, int detectionToAlertDelayMilliseconds);

        void Start();

        void Stop();

        Action<Repository> OnChange { get; set; }
    }
}