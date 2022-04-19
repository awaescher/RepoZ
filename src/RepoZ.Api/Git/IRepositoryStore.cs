namespace RepoZ.Api.Git
{
    using System.Collections.Generic;

    public interface IRepositoryStore
    {
        IEnumerable<string> Get();

        void Set(IEnumerable<string> paths);
    }
}