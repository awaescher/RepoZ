using System;
using System.Collections.Generic;
using ExpressionStringEvaluator.VariableProviders;
using RepoZ.Api.Git;

namespace RepoZ.Api.Common.IO
{
    public class CustomEnvironmentVariableVariableProvider : IVariableProvider<Repository>
    {
        private readonly Func<Repository, Dictionary<string, string>> _getRepoEnvironmentVariables;

        public CustomEnvironmentVariableVariableProvider(Func<Repository, Dictionary<string, string>> getRepoEnvironmentVariables)
        {
            _getRepoEnvironmentVariables = getRepoEnvironmentVariables ?? throw new ArgumentNullException(nameof(getRepoEnvironmentVariables));
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

            var envVars = _getRepoEnvironmentVariables.Invoke(context);

            if (envVars.ContainsKey(envKey))
            {
                return envVars[envKey];
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
    }
}