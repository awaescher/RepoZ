namespace RepoZ.Api
{
    using System;
    using System.Collections.Generic;
    using RepoZ.Api.Git;

    public interface IRepositorySearch
    {
        IEnumerable<string> Search(string query);
    }
}