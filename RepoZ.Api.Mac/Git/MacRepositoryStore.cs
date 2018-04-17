using System;
using System.IO;
using RepoZ.Api.Common;
using RepoZ.Api.Common.Git;

namespace RepoZ.Api.Mac.Git
{
    public class MacRepositoryStore : FileRepositoryStore
    {
        public MacRepositoryStore(IErrorHandler errorHandler)
            : base(errorHandler)
        {
        }

        public override string GetFileName() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoZ/Repositories.cache");
    }
}
