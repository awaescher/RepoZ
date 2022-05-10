namespace RepoZ.Api.Common.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using DotNetEnv;
    using ExpressionStringEvaluator.VariableProviders;
    using RepoZ.Api.Git;

    public class CustomEnvironmentVariableVariableProvider : IVariableProvider<Repository>
    {
        private static readonly Dictionary<string, string> _emptyDictionary = new Dictionary<string, string>(0);
        private readonly IFileSystem _fileSystem;

        public CustomEnvironmentVariableVariableProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        private const string PREFIX = "Env.";

        /// <inheritdoc cref="IVariableProvider.CanProvide"/>
        public bool CanProvide(string key)
        {
            if (key is null)
            {
                return false;
            }

            if (!key.StartsWith(PREFIX, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            var prefixLength = PREFIX.Length;
            if (key.Length <= prefixLength)
            {
                return false;
            }

            var envKey = key.Substring(prefixLength, key.Length - prefixLength);

            return !string.IsNullOrWhiteSpace(envKey);
        }

        public string Provide(Repository context, string key, string arg)
        {
            var prefixLength = PREFIX.Length;
            var envKey = key.Substring(prefixLength, key.Length - prefixLength);
            var envVars = GetRepoEnvironmentVariables(context);

            if (envVars != null)
            {
                if (envVars.ContainsKey(envKey))
                {
                    return envVars[envKey];
                }
            }

            var result = Environment.GetEnvironmentVariable(envKey) ?? string.Empty;

            return result;
        }

        /// <inheritdoc cref="IVariableProvider.Provide"/>
        public string Provide(string key, string arg)
        {
            var prefixLength = PREFIX.Length;
            var envKey = key.Substring(prefixLength, key.Length - prefixLength);
            var result = Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
            return result;
        }

        private Dictionary<string, string> GetRepoEnvironmentVariables(Repository repository)
        {
            var repozEnvFile = Path.Combine(repository.Path, ".git", "repoz.env");

            // todo use FileSystemIFileSystem
            if (!_fileSystem.File.Exists(repozEnvFile))
            {
                return _emptyDictionary;
            }

            try
            {
                return DotNetEnv.Env.Load(repozEnvFile, new DotNetEnv.LoadOptions(setEnvVars: false)).ToDictionary();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return _emptyDictionary;
        }

    }
}