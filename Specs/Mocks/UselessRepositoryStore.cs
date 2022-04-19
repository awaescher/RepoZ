namespace Specs.Mocks
{
    using System.Collections.Generic;
    using RepoZ.Api.Git;
    using System;

    internal class UselessRepositoryStore : IRepositoryStore
    {
        public IEnumerable<string> Get()
        {
            return Array.Empty<string>();
        }

        public void Set(IEnumerable<string> paths) { }
    }
}