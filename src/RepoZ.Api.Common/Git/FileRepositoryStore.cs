namespace RepoZ.Api.Common.Git
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using RepoZ.Api.Common;
    using RepoZ.Api.Git;

    public abstract class FileRepositoryStore : IRepositoryStore
    {
        private readonly IErrorHandler _errorHandler;

        protected FileRepositoryStore(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public abstract string GetFileName();

        public IEnumerable<string> Get(string file)
        {
            if (!UseFilePersistence)
            {
                return Array.Empty<string>();
            }

            if (File.Exists(file))
            {
                try
                {
                    return File.ReadAllLines(file);
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
            var path = Directory.GetParent(file).FullName;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                File.WriteAllLines(GetFileName(), paths.ToArray());
            }
            catch (Exception ex)
            {
                _errorHandler.Handle(ex.Message);
            }
        }

        public bool UseFilePersistence { get; set; } = true;
    }
}