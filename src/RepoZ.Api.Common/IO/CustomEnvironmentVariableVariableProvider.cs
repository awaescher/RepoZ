namespace RepoZ.Api.Common.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading;
    using DotNetEnv;
    using ExpressionStringEvaluator.VariableProviders;
    using RepoZ.Api.Common.IO.ExpressionEvaluator;
    using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
    using Repository = RepoZ.Api.Git.Repository;

    public static class RepoZVariableProviderStore
    {
        public static AsyncLocal<Scope> VariableScope = new AsyncLocal<Scope>();

        public static IDisposable Push(List<Variable> vars)
        {
            VariableScope.Value = new Scope(VariableScope.Value, vars);
            return VariableScope.Value;
        }

        public static List<Variable> Clone()
        {
            return VariableScope.Value.Variables.ToList(); //todo
        }
    }

    public class Scope : IDisposable
    {
        // private readonly LoggerFactoryScopeProvider _provider;
        private bool _isDisposed;

        private Scope()
        {
            Parent = null;
            Variables = null;
        }

        public Scope(Scope parent, List<Variable> variables)
        {
            // _provider = provider;
            // State = state;
            Parent = parent;
            Variables = variables;
        }

        public static Scope Empty { get; } = new Scope();

        public Scope Parent { get; }

        public List<Variable> Variables { get; }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                RepoZVariableProviderStore.VariableScope.Value = Parent;
                _isDisposed = true;
            }
        }
    }


    public class RepoZVariableProvider : IVariableProvider
    {
        private const string PREFIX = "var.";

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

        /// <inheritdoc cref="IVariableProvider.Provide"/>
        public string Provide(string key, string arg)
        {
            var prefixLength = PREFIX.Length;
            var envKey = key.Substring(prefixLength, key.Length - prefixLength);

            Scope scope = RepoZVariableProviderStore.VariableScope.Value;

            while (true)
            {
                if (scope == null)
                {
                    return string.Empty;
                }

                if (TryGetValueFromScope(scope, envKey, out var result))
                {
                    return result;
                }

                scope = scope.Parent;
            }
        }

        private static bool TryGetValueFromScope(in Scope scope, string key, out string value)
        {
            if (scope?.Variables == null)
            {
                value = string.Empty;
                return false;
            }

            Variable var = scope.Variables.FirstOrDefault(x => x.Name == key);

            if (var != null)
            {
                if (var.Enabled == "false") // todo
                {
                    value = string.Empty;
                    return true;
                }

                value = var.Value;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }

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