namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using DotNetEnv;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoZ.Api.IO;
using Repository = RepoZ.Api.Git.Repository;
using RepositoryAction = RepoZ.Api.Git.RepositoryAction;

public class RepositorySpecificConfiguration
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly DynamicRepositoryActionDeserializer _appSettingsDeserializer;
    private readonly RepositoryExpressionEvaluator _repoExpressionEvaluator;
    private readonly ActionMapperComposition _actionMapper;
    private readonly ITranslationService _translationService;
    private readonly IErrorHandler _errorHandler;

    public const string FILENAME = "RepositoryActionsV2.json";

    public RepositorySpecificConfiguration(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        DynamicRepositoryActionDeserializer appsettingsDeserializer,
        RepositoryExpressionEvaluator repoExpressionEvaluator,
        ActionMapperComposition actionMapper,
        ITranslationService translationService,
        IErrorHandler errorHandler)
    {
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _appSettingsDeserializer = appsettingsDeserializer ?? throw new ArgumentNullException(nameof(appsettingsDeserializer));
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _actionMapper = actionMapper ?? throw new ArgumentNullException(nameof(actionMapper));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    public IEnumerable<string> GetTags(RepoZ.Api.Git.Repository repository)
    {
        if (repository == null)
        {
            yield break;
        }

        // load default file
        RepositoryActionConfiguration2 rootFile = null;
        RepositoryActionConfiguration2 repoSpecificConfig = null;

        var filename = Path.Combine(_appDataPathProvider.GetAppDataPath(), FILENAME);
        if (!_fileSystem.File.Exists(filename))
        {
            yield break;
        }

        try
        {
            var content = _fileSystem.File.ReadAllText(filename, Encoding.UTF8);
            rootFile = _appSettingsDeserializer.Deserialize(content);
        }
        catch (Exception)
        {
            yield break;
        }


        Redirect redirect = rootFile.Redirect;
        if (!string.IsNullOrWhiteSpace(redirect?.Filename))
        {
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
                    catch (Exception)
                    {
                        yield break;
                    }
                }
            }
        }

        List<Variable> EvaluateVariables(IEnumerable<Variable> vars)
        {
            if (vars == null)
            {
                return new List<Variable>(0);
            }

            return vars
                   .Where(v => IsEnabled(v.Enabled, true, repository))
                   .Select(v => new Variable()
                   {
                       Name = v.Name,
                       Enabled = "true",
                       Value = Evaluate(v.Value, repository),
                   })
                   .ToList();
        }

        using IDisposable rootVariables = RepoZVariableProviderStore.Push(EvaluateVariables(rootFile.Variables));

        Dictionary<string, string> envVars = null;

        // load repo specific environment variables
        foreach (FileReference fileRef in rootFile.RepositorySpecificEnvironmentFiles)
        {
            if (envVars != null)
            {
                continue;
            }

            if (fileRef == null || !IsEnabled(fileRef.When, true, repository))
            {
                continue;
            }

            var f = Evaluate(fileRef.Filename, repository);
            if (!_fileSystem.File.Exists(f))
            {
                // log warning?
                continue;
            }

            try
            {
                envVars = DotNetEnv.Env.Load(f, new DotNetEnv.LoadOptions(setEnvVars: false)).ToDictionary();
            }
            catch (Exception e)
            {
                // log warning
            }
        }

        using IDisposable repoSpecificEnvVariables = CoenRepoZEnvironmentVarialeStore.Set(envVars);

        // load repo specific config
        foreach (FileReference fileRef in rootFile.RepositorySpecificConfigFiles)
        {
            if (repoSpecificConfig != null)
            {
                continue;
            }

            if (fileRef == null || !IsEnabled(fileRef.When, true, repository))
            {
                continue;
            }

            var f = Evaluate(fileRef.Filename, repository);
            if (!_fileSystem.File.Exists(f))
            {
                // warning
                continue;
            }

            // todo redirect

            try
            {
                var content = _fileSystem.File.ReadAllText(f, Encoding.UTF8);
                repoSpecificConfig = _appSettingsDeserializer.Deserialize(content);
            }
            catch (Exception)
            {
                // warning.
            }
        }

        using IDisposable repoSepecificVariables = RepoZVariableProviderStore.Push(EvaluateVariables(repoSpecificConfig?.Variables));

        // load variables global
        foreach (TagsCollection tagsCollection in new[] { rootFile.TagsCollection, repoSpecificConfig?.TagsCollection, })
        {
            if (tagsCollection == null)
            {
                continue;
            }

            using IDisposable disposable = RepoZVariableProviderStore.Push(EvaluateVariables(tagsCollection.Variables));

            // add variables to set
            // rootFile.ActionsCollection.Variables
            foreach (RepositoryActionTag action in tagsCollection.Tags)
            {
                if (!IsEnabled(action.When, true, repository))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(action.Tag))
                {
                    yield return action.Tag;
                }
            }
        }
    }

    public IEnumerable<RepositoryAction> CreateActions(params RepoZ.Api.Git.Repository[] repositories)
    {
        if (repositories == null)
        {
            throw new ArgumentNullException(nameof(repositories));
        }

        Repository singleRepository = null;
        var multiSelectRequired = repositories.Length > 1;
        if (!multiSelectRequired)
        {
            singleRepository = repositories.FirstOrDefault();
        }

        // load default file
        RepositoryActionConfiguration2 rootFile = null;
        RepositoryActionConfiguration2 repoSpecificConfig = null;

        var filename = Path.Combine(_appDataPathProvider.GetAppDataPath(), FILENAME);
        if (!_fileSystem.File.Exists(filename))
        {
            foreach (RepositoryAction failingItem in CreateFailing(new Exception(FILENAME + " file does not exists"), filename))
            {
                yield return failingItem;
            }

            yield break;
        }

        Exception exception = null;
        try
        {
            var content = _fileSystem.File.ReadAllText(filename, Encoding.UTF8);
            rootFile = _appSettingsDeserializer.Deserialize(content);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (exception != null)
        {
            foreach (RepositoryAction failingItem in CreateFailing(exception, filename))
            {
                yield return failingItem;
            }

            yield break;
        }

        Redirect redirect = rootFile.Redirect;
        if (!string.IsNullOrWhiteSpace(redirect?.Filename))
        {
            if (IsEnabled(redirect?.Enabled, true, null))
            {
                filename = Evaluate(redirect?.Filename, null);
                if (_fileSystem.File.Exists(filename))
                {
                    exception = null;
                    try
                    {
                        var content = _fileSystem.File.ReadAllText(filename, Encoding.UTF8);
                        rootFile = _appSettingsDeserializer.Deserialize(content);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    if (exception != null)
                    {
                        foreach (RepositoryAction failingItem in CreateFailing(exception, filename))
                        {
                            yield return failingItem;
                        }

                        yield break;
                    }
                }
            }
        }

        List<Variable> EvaluateVariables(IEnumerable<Variable> vars)
        {
            if (vars == null)
            {
                return new List<Variable>(0);
            }

            return vars
                   .Where(v => IsEnabled(v.Enabled, true, singleRepository))
                   .Select(v => new Variable()
                       {
                           Name = v.Name,
                           Enabled = "true",
                           Value = Evaluate(v.Value, singleRepository),
                       })
                   .ToList();
        }

        using IDisposable rootVariables = RepoZVariableProviderStore.Push(EvaluateVariables(rootFile.Variables));

        Dictionary<string, string> envVars = null;

        if (!multiSelectRequired)
        {
            // load repo specific environment variables
            foreach (FileReference fileRef in rootFile.RepositorySpecificEnvironmentFiles)
            {
                if (envVars != null)
                {
                    continue;
                }

                if (fileRef == null || !IsEnabled(fileRef.When, true, singleRepository))
                {
                    continue;
                }

                var f = Evaluate(fileRef.Filename, singleRepository);
                if (!_fileSystem.File.Exists(f))
                {
                    // log warning?
                    continue;
                }

                try
                {
                    envVars = DotNetEnv.Env.Load(f, new DotNetEnv.LoadOptions(setEnvVars: false)).ToDictionary();
                }
                catch (Exception e)
                {
                    // log warning
                }
            }
        }

        using IDisposable repoSpecificEnvVariables = CoenRepoZEnvironmentVarialeStore.Set(envVars);

        if (!multiSelectRequired)
        {
            // load repo specific config
            foreach (FileReference fileRef in rootFile.RepositorySpecificConfigFiles)
            {
                if (repoSpecificConfig != null)
                {
                    continue;
                }

                if (fileRef == null || !IsEnabled(fileRef.When, true, singleRepository))
                {
                    continue;
                }

                var f = Evaluate(fileRef.Filename, singleRepository);
                if (!_fileSystem.File.Exists(f))
                {
                    // warning
                    continue;
                }

                // todo redirect

                try
                {
                    var content = _fileSystem.File.ReadAllText(f, Encoding.UTF8);
                    repoSpecificConfig = _appSettingsDeserializer.Deserialize(content);
                }
                catch (Exception)
                {
                    // warning.
                }
            }
        }

        using IDisposable repoSepecificVariables = RepoZVariableProviderStore.Push(EvaluateVariables(repoSpecificConfig?.Variables));

        // load variables global
        foreach (ActionsCollection actionsCollection in new []{ rootFile.ActionsCollection , repoSpecificConfig?.ActionsCollection, })
        {
            if (actionsCollection == null)
            {
                continue;
            }

            using IDisposable disposable = RepoZVariableProviderStore.Push(EvaluateVariables(actionsCollection.Variables));

            // add variables to set
            // rootFile.ActionsCollection.Variables
            foreach (Data.RepositoryAction action in actionsCollection.Actions)
            {
                using IDisposable x = RepoZVariableProviderStore.Push(EvaluateVariables(action.Variables));

                if (multiSelectRequired)
                {
                    var actionNotCapableForMultipleRepos = repositories.Any(repo => !IsEnabled(action.MultiSelectEnabled, false, repo));
                    if (actionNotCapableForMultipleRepos)
                    {
                        continue;
                    }
                }

                IEnumerable<RepositoryAction> result = _actionMapper.Map(action, repositories);
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

    private IEnumerable<RepositoryAction> CreateFailing(Exception ex, string filename)
    {
        yield return new RepositoryAction()
            {
                Name = _translationService.Translate("Could not read repository actions"),
                CanExecute = false,
            };

        yield return new RepositoryAction()
            {
                Name = ex.Message,
                CanExecute = false,
            };

        if (!string.IsNullOrWhiteSpace(filename))
        {
            yield return new RepositoryAction()
                {
                    Name = _translationService.Translate("Fix"),
                    Action = (_, _) => ProcessHelper.StartProcess(_fileSystem.Path.GetDirectoryName(filename), string.Empty, _errorHandler),
                };
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