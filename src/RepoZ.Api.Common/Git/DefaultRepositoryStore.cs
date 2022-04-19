namespace RepoZ.Api.Common.Git
{
    using System;
    using System.IO;
    using RepoZ.Api.IO;

    public class DefaultRepositoryStore : FileRepositoryStore
    {
        public DefaultRepositoryStore(IErrorHandler errorHandler, IAppDataPathProvider appDataPathProvider)
            : base(errorHandler)
        {
            AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        }

        public override string GetFileName()
        {
            return Path.Combine(AppDataPathProvider.GetAppDataPath(), "Repositories.cache");
        }

        public IAppDataPathProvider AppDataPathProvider { get; }
    }
}