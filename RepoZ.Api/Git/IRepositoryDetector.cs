namespace RepoZ.Api.Git
{
    using System;

    public interface IRepositoryDetector
    {
        void Setup(string path, int detectionToAlertDelayMilliseconds);

        void Start();

        void Stop();

        Action<Repository> OnAddOrChange { get; set; }

        Action<string> OnDelete { get; set; }
    }
}