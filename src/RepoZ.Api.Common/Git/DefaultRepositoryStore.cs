namespace RepoZ.Api.Common.Git
{
    using System;
    using System.IO;
    using RepoZ.Api.IO;

    public class DefaultRepositoryStore : FileRepositoryStore
    {
        private readonly string _fullFilename;

        public DefaultRepositoryStore(IErrorHandler errorHandler, IAppDataPathProvider appDataPathProvider)
            : base(errorHandler)
        {
            AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
            _fullFilename = Path.Combine(AppDataPathProvider.GetAppDataPath(), "Repositories.cache");
        }

        public override string GetFileName()
        {
            return _fullFilename;
        }

        public IAppDataPathProvider AppDataPathProvider { get; }
    }
}