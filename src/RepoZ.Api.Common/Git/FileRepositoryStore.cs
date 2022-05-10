namespace RepoZ.Api.Common.Git
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using RepoZ.Api.Common;
    using RepoZ.Api.Git;

    public abstract class FileRepositoryStore : IRepositoryStore
    {
        private readonly IErrorHandler _errorHandler;
        private protected readonly IFileSystem FileSystem;

        protected FileRepositoryStore(IErrorHandler errorHandler, IFileSystem fileSystem)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public abstract string GetFileName();

        public IEnumerable<string> Get(string file)
        {
            if (!UseFilePersistence)
            {
                return Array.Empty<string>();
            }

            if (FileSystem.File.Exists(file))
            {
                try
                {
                    return FileSystem.File.ReadAllLines(file);
                }
                catch (Exception ex)
                {
                    _errorHandler.Handle(ex.Message);
                }
            }

            return Array.Empty<string>();
        }

        public IEnumerable<string> Get()
        {
            var file = GetFileName();
            return Get(file);
        }

        public void Set(IEnumerable<string> paths)
        {
            if (!UseFilePersistence)
            {
                return;
            }

            var file = GetFileName();
            var path = FileSystem.Directory.GetParent(file).FullName;

            if (!FileSystem.Directory.Exists(path))
            {
                FileSystem.Directory.CreateDirectory(path);
            }

            try
            {
                FileSystem.File.WriteAllLines(GetFileName(), paths.ToArray());
            }
            catch (Exception ex)
            {
                _errorHandler.Handle(ex.Message);
            }
        }

        public bool UseFilePersistence { get; set; } = true;
    }
}