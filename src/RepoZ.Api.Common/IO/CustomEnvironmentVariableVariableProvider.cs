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

    public static class RepoZEnvironmentVariableStore
    {
        public static readonly AsyncLocal<Dictionary<string, string>> EnvVars = new AsyncLocal<Dictionary<string, string>>();

        public static IDisposable Set(Dictionary<string, string> envVars)
        {
            EnvVars.Value = envVars;
            return new ExecuteOnDisposed(() => EnvVars.Value = null);
        }

        public static Dictionary<string, string> Get(Repository repository)
        {
            return EnvVars.Value ?? new Dictionary<string, string>(0);
        }
    }

    public class ExecuteOnDisposed : IDisposable
    {
        private readonly Func<Dictionary<string, string>> _func;

        public ExecuteOnDisposed(Func<Dictionary<string, string>> func)
        {
            _func = func;
        }

        public void Dispose()
        {
            _func?.Invoke();
        }
    }

    public static class RepoZVariableProviderStore
    {
        public static readonly AsyncLocal<Scope> VariableScope = new AsyncLocal<Scope>();

        public static IDisposable Push(List<Variable> vars)
        {
            VariableScope.Value = new Scope(VariableScope.Value, vars);
            return VariableScope.Value;
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

    public class VariableCollection
    {
        private readonly List<EvaluatingVariable> _variables;

        public VariableCollection(List<Variable> variables)
        {
            _variables = variables.Select(x => new EvaluatingVariable(x.Name, x.Value, x.Enabled)).ToList();
        }

        // public string GetValueWhenEnabled(string key, Scope parent)
        // {
        //     //evaluate all
        //     for (int i = 0; i < _variables.Count; i++)
        //     {
        //         _variables[i].Evaluate(parent, _variables.Take(i));
        //         if (_variables[i].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
        //         {
        //             if (_variables[i].IsEnabled(parent))
        //             {
        //                 return _variables.GetValue(parent);
        //             }
        //
        //             return string.Empty;
        //         }
        //     }
        // }

    }


    public class EvaluatingVariable
    {
        public string Name { get; }
        private readonly string _value;
        private readonly string _enabled;

        private bool? _isEnabled = null;
        private string _evaluatedValue = null;

        public EvaluatingVariable(string name, string value, string enabled)
        {
            Name = name;
            _value = value;
            _enabled = enabled;
        }

        public (string key, string value, bool enabled) Evaluate(Scope parent, IEnumerable<EvaluatingVariable> variables)
        {
            if (!_isEnabled.HasValue)
            {
                _isEnabled = true;
            }

            if (_evaluatedValue == null)
            {

            }

            return (Name, _evaluatedValue, _isEnabled.Value!);
        }

        // public bool IsEnabled(Scope parent)
        // {
        // }
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

            Variable var = scope.Variables.FirstOrDefault(x => x.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));

            if (var != null)
            {
                if (var.Enabled.Equals("false",StringComparison.CurrentCultureIgnoreCase))
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

    public class CustomEnvironmentVariableVariableProvider : IVariableProvider<RepositoryContext>
    {
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

        public string Provide(RepositoryContext context, string key, string arg)
        {
            var prefixLength = PREFIX.Length;
            var envKey = key.Substring(prefixLength, key.Length - prefixLength);

            Repository singleContext = context?.Repositories.SingleOrDefault();
            if (singleContext != null)
            {
                Dictionary<string, string> envVars = GetRepoEnvironmentVariables(singleContext);

                if (envVars != null)
                {
                    if (envVars.ContainsKey(envKey))
                    {
                        return envVars[envKey];
                    }
                }
            }

            return Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
        }

        /// <inheritdoc cref="IVariableProvider.Provide"/>
        public string Provide(string key, string arg)
        {
            var prefixLength = PREFIX.Length;
            var envKey = key.Substring(prefixLength, key.Length - prefixLength);
            var result = Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
            return result;
        }

        private Dictionary<string, string> GetRepoEnvironmentVariables(Repository context)
        {
            return RepoZEnvironmentVariableStore.Get(context);
        }
    }
}