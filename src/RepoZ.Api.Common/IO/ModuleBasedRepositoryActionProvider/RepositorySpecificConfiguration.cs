namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Resources;
using System.Text;
using DotNetEnv;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using static System.Collections.Specialized.BitVector32;
using RepositoryAction = RepoZ.Api.Git.RepositoryAction;

public class RepositorySpecificConfiguration
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly DynamicRepositoryActionDeserializer _appSettingsDeserializer;
    private readonly RepositoryExpressionEvaluator _repoExpressionEvaluator;
    private readonly ActionMapperComposition _actionMapper;

    public const string FILENAME = "RepositoryActionsV2.json";

    public RepositorySpecificConfiguration(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        DynamicRepositoryActionDeserializer appsettingsDeserializer,
        RepositoryExpressionEvaluator repoExpressionEvaluator,
        ActionMapperComposition actionMapper)
    {
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _appSettingsDeserializer = appsettingsDeserializer ?? throw new ArgumentNullException(nameof(appsettingsDeserializer));
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _actionMapper = actionMapper ?? throw new ArgumentNullException(nameof(actionMapper));
    }

    public IEnumerable<RepositoryAction> Create(params RepoZ.Api.Git.Repository[] repository)
    {
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        Repository singleRepository = null;
        var multiSelectRequired = repository.Length > 1;
        if (!multiSelectRequired)
        {
            singleRepository = repository.FirstOrDefault();
        }

        // load default file
        RepositoryActionConfiguration2 rootFile = null;
        RepositoryActionConfiguration2 repoSpecificConfig = null;

        var filename = Path.Combine(_appDataPathProvider.GetAppDataPath(), FILENAME);
        if (!_fileSystem.File.Exists(filename))
        {
            throw new Exception(FILENAME + " file does not exists");
        }

        try
        {
            var content = _fileSystem.File.ReadAllText(filename, Encoding.UTF8);
            rootFile = _appSettingsDeserializer.Deserialize(content);
        }
        catch (Exception e)
        {
            throw new Exception("Could not deserialize appsettings.json", e);
        }

        if (rootFile == null)
        {
            throw new Exception("Could not deserialize appsettings.json");
        }

        Redirect redirect = rootFile.Redirect;
        if (IsEnabled(redirect?.Enabled, true, null))
        {
            filename = Evaluate(redirect?.Filename, null);
            if (_fileSystem.File.Exists(filename))
            {
                try
                {
                    var content = _fileSystem.File.ReadAllText(filename, Encoding.UTF8);
                    rootFile = _appSettingsDeserializer.Deserialize(content);
                }
                catch (Exception e)
                {
                    throw new Exception("Could not deserialize appsettings.json", e);
                }

                if (rootFile == null)
                {
                    throw new Exception("Could not deserialize appsettings.json");
                }
            }
        }

        using IDisposable rootVariables = RepoZVariableProviderStore.Push(rootFile.Variables);

        Dictionary<string, string> envVars = null;

        if (!multiSelectRequired)
        {
            // load repo specific environment variables
            for (var i = 0; i < rootFile.RepositorySpecificEnvironmentFiles.Count; i++)
            {
                if (envVars != null)
                {
                    continue;
                }

                FileReference fileRef = rootFile.RepositorySpecificEnvironmentFiles[i];
                if (fileRef == null || !IsEnabled(fileRef.When, true, singleRepository))
                {
                    continue;
                }

                var f = Evaluate(fileRef.Filename, singleRepository);
                if (!_fileSystem.File.Exists(f))
                {
                    continue;
                }
                //private Dictionary<string, string> GetRepoEnvironmentVariables(Repository repository)

                //var repozEnvFile = Path.Combine(repository.Path, ".git", "repoz.env");

                try
                {
                    envVars = DotNetEnv.Env.Load(f, new DotNetEnv.LoadOptions(setEnvVars: false)).ToDictionary();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        using IDisposable repoSpecificEnvVariables = CoenRepoZEnvironmentVarialeStore.Set(envVars);

        if (!multiSelectRequired)
        {
            // load repo specific config
            for (int i = 0; i < rootFile.RepositorySpecificConfigFiles.Count; i++)
            {
                if (repoSpecificConfig != null)
                {
                    continue;
                }

                FileReference fileRef = rootFile.RepositorySpecificConfigFiles[i];
                if (fileRef == null || !IsEnabled(fileRef.When, true, singleRepository))
                {
                    continue;
                }

                var f = Evaluate(fileRef.Filename, singleRepository);
                if (!_fileSystem.File.Exists(f))
                {
                    continue;
                }

                try
                {
                    var content = _fileSystem.File.ReadAllText(f, Encoding.UTF8);
                    repoSpecificConfig = _appSettingsDeserializer.Deserialize(content);
                }
                catch (Exception)
                {
                    // log, no warning.
                    // throw new Exception("Could not deserialize appsettings.json", e);
                }
            }
        }

        using IDisposable repoSepecificVariables = RepoZVariableProviderStore.Push(repoSpecificConfig?.Variables);

        // load variables global
        if (rootFile.ActionsCollection != null)
        {
            using IDisposable disposable = RepoZVariableProviderStore.Push(rootFile.ActionsCollection.Variables);

            // add variables to set
            // rootFile.ActionsCollection.Variables
            foreach (Data.RepositoryAction action in rootFile.ActionsCollection.Actions)
            {
                //coen
                using IDisposable x = RepoZVariableProviderStore.Push(action.Variables);

                if (multiSelectRequired)
                {
                    var actionNotCapableForMultipleRepos = repository.Any(repo => !IsEnabled(action.MultiSelectEnabled, false, repo));
                    if (actionNotCapableForMultipleRepos)
                    {
                        continue;
                    }
                }

                IEnumerable<RepositoryAction> result = _actionMapper.Map(action, singleRepository ?? repository.First() /*todo*/);
                if (result == null)
                {
                    continue;
                }

                foreach (RepositoryAction singleItem in result)
                {
                    yield return singleItem;
                }
            }
        }
    }

    private string Evaluate(string input, Repository repository)
    {
        return _repoExpressionEvaluator.EvaluateStringExpression(input, repository);
    }

    private bool IsEnabled(string booleanExpression, bool defaultWhenNullOrEmpty, Repository repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _repoExpressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
    }
}