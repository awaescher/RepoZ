namespace RepoZ.Api.Common.Git
{
    using RepoZ.Api.Git;
    using RepoZ.Api.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DefaultRepositoryIgnoreStore : FileRepositoryStore, IRepositoryIgnoreStore
    {
        private List<string> _ignores = null;
        private IEnumerable<IgnoreRule> _rules;
        private readonly object _lock = new object();
        private readonly string _fullFilename;

        public DefaultRepositoryIgnoreStore(IErrorHandler errorHandler, IAppDataPathProvider appDataPathProvider)
            : base(errorHandler)
        {
            AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
            _fullFilename = Path.Combine(AppDataPathProvider.GetAppDataPath(), "Repositories.ignore");
        }

        public override string GetFileName()
        {
            return _fullFilename;
        }

        public void IgnoreByPath(string path)
        {
            Ignores.Add(path);
            UpdateRules();

            Set(Ignores);
        }

        public bool IsIgnored(string path)
        {
            if (_rules is null)
            {
                UpdateRules();
            }

            return _rules.Any(r => r.IsIgnored(path));
        }

        public void Reset()
        {
            Ignores.Clear();
            UpdateRules();

            Set(Ignores);
        }

        private List<string> Ignores
        {
            get
            {
                if (_ignores == null)
                {
                    lock (_lock)
                    {
                        _ignores = Get()?.ToList() ?? new List<string>();
                        UpdateRules();
                    }
                }

                return _ignores;
            }
        }

        private void UpdateRules()
        {
            _rules = Ignores.Select(i => new IgnoreRule(i));
        }

        public IAppDataPathProvider AppDataPathProvider { get; }
    }
}