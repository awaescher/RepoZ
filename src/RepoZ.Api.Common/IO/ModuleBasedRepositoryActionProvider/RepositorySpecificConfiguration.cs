namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using DotNetEnv;
using JetBrains.Annotations;
using LibGit2Sharp;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoZ.Api.IO;
using Repository = RepoZ.Api.Git.Repository;
using RepositoryAction = RepoZ.Api.Git.RepositoryAction;

public interface IRepositoryTagsFactory
{
    IEnumerable<string> GetTags(RepoZ.Api.Git.Repository repository);
}

public class ConfigurationFileNotFoundException : Exception
{
    public ConfigurationFileNotFoundException(string filename)
    {
        Filename = filename;
    }

    protected ConfigurationFileNotFoundException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ConfigurationFileNotFoundException(string filename, string message) : base(message)
    {
        Filename = filename;
    }

    public ConfigurationFileNotFoundException(string filename, string message, Exception innerException) : base(message, innerException)
    {
        Filename = filename;
    }

    public string Filename { get; set; }
}

public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string filename)
    {
        Filename = filename;
    }

    protected InvalidConfigurationException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidConfigurationException(string filename, string message) : base(message)
    {
        Filename = filename;
    }

    public InvalidConfigurationException(string filename, string message, Exception innerException) : base(message, innerException)
    {
        Filename = filename;
    }

    public string Filename { get; set; }
}

public class RepositoryConfigurationReader
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly DynamicRepositoryActionDeserializer _appSettingsDeserializer;
    private readonly RepositoryExpressionEvaluator _repoExpressionEvaluator;

    public const string FILENAME = "RepositoryActionsV2.json";

    public RepositoryConfigurationReader(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        DynamicRepositoryActionDeserializer appsettingsDeserializer,
        RepositoryExpressionEvaluator repoExpressionEvaluator
        )
    {
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _appSettingsDeserializer = appsettingsDeserializer ?? throw new ArgumentNullException(nameof(appsettingsDeserializer));
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
    }

    public (Dictionary<string, string> envVars, List<Variable> Variables, List<ActionsCollection> actions, List<TagsCollection> tags) Get(params RepoZ.Api.Git.Repository[] repositories)
    {
        if (repositories == null || !repositories.Any())
        {
            return (null, null, null, null);
        }

        Repository repository = repositories.FirstOrDefault(); //todo
        if (repository == null)
        {
            return (null, null, null, null);
        }

        Repository singleRepository = null;
        var multipleRepositoriesSelected = repositories.Length > 1;
        if (!multipleRepositoriesSelected)
        {
            singleRepository = repositories.FirstOrDefault();
        }

        var variables = new List<Variable>();
        Dictionary<string, string> envVars = null;
        var actions = new List<ActionsCollection>();
        var tags = new List<TagsCollection>();

        // load default file
        RepositoryActionConfiguration rootFile = null;
        RepositoryActionConfiguration repoSpecificConfig = null;

        var filename = Path.Combine(_appDataPathProvider.GetAppDataPath(), FILENAME);
        if (!_fileSystem.File.Exists(filename))
        {
            throw new ConfigurationFileNotFoundException(filename);
        }

        try
        {
            var content = _fileSystem.File.ReadAllText(filename, Encoding.UTF8);
            rootFile = _appSettingsDeserializer.Deserialize(content);
        }
        catch (Exception e)
        {
            throw new InvalidConfigurationException(filename, e.Message, e);
        }
        
        Redirect redirect = rootFile?.Redirect;
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
                    catch (Exception e)
                    {
                        throw new InvalidConfigurationException(filename, e.Message, e);
                    }
                }
            }
        }

        if (rootFile == null)
        {
            return (null, null, null, null);
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

        List<Variable> list = EvaluateVariables(rootFile.Variables);
        variables.AddRange(list);
        using IDisposable rootVariables = RepoZVariableProviderStore.Push(list);

        if (!multipleRepositoriesSelected)
        {
            // load repo specific environment variables
            foreach (FileReference fileRef in rootFile.RepositorySpecificEnvironmentFiles.Where(fileRef => fileRef != null))
            {
                if (envVars != null)
                {
                    continue;
                }

                if (!IsEnabled(fileRef.When, true, repository))
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
                catch (Exception)
                {
                    // log warning
                }
            }
        }

        using IDisposable repoSpecificEnvVariables = RepoZEnvironmentVariableStore.Set(envVars);

        if (!multipleRepositoriesSelected)
        {
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
        }
        
        List<Variable> list2 = EvaluateVariables(repoSpecificConfig?.Variables);
        variables.AddRange(list2);
        using IDisposable repoSepecificVariables = RepoZVariableProviderStore.Push(list2);

        actions.Add(rootFile.ActionsCollection);
        if (repoSpecificConfig?.ActionsCollection != null)
        {
            actions.Add(repoSpecificConfig.ActionsCollection);
        }

        tags.Add(rootFile.TagsCollection);
        if (repoSpecificConfig?.TagsCollection != null)
        {
            tags.Add(repoSpecificConfig.TagsCollection);
        }

        return (envVars, variables, actions, tags);
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

public class RepositoryTagsConfigurationFactory : IRepositoryTagsFactory 
{
    private readonly RepositoryExpressionEvaluator _repoExpressionEvaluator;
    private readonly RepositoryConfigurationReader _repoConfigReader;

    public RepositoryTagsConfigurationFactory(
        RepositoryExpressionEvaluator repoExpressionEvaluator,
        RepositoryConfigurationReader repoConfigReader)
    {
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _repoConfigReader = repoConfigReader ?? throw new ArgumentNullException(nameof(repoConfigReader));
    }

    public IEnumerable<string> GetTags(RepoZ.Api.Git.Repository repository)
    {
        return GetTagsInner(repository).Distinct();
    }

    private IEnumerable<string> GetTagsInner(RepoZ.Api.Git.Repository repository)
    {
        if (repository == null)
        {
            yield break;
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

        Dictionary<string, string> repositoryEnvVars;
        List< Variable > variables;
        List<TagsCollection> tags;

        try
        {
            (repositoryEnvVars,  variables, _,  tags) = _repoConfigReader.Get(repository);
        }
        catch (Exception)
        {
             // todo, log
             yield break;
        }

        using IDisposable d1 = RepoZVariableProviderStore.Push(EvaluateVariables(variables));
        using IDisposable d2 = RepoZEnvironmentVariableStore.Set(repositoryEnvVars);

        foreach (TagsCollection tagsCollection in tags.Where(t => t != null))
        {
            using IDisposable d3 = RepoZVariableProviderStore.Push(EvaluateVariables(tagsCollection.Variables));

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

public class RepositorySpecificConfiguration
{
    private readonly IFileSystem _fileSystem;
    private readonly RepositoryExpressionEvaluator _repoExpressionEvaluator;
    private readonly ActionMapperComposition _actionMapper;
    private readonly ITranslationService _translationService;
    private readonly IErrorHandler _errorHandler;
    private readonly RepositoryConfigurationReader _repoConfigReader;

    public RepositorySpecificConfiguration(
        IFileSystem fileSystem,
        RepositoryExpressionEvaluator repoExpressionEvaluator,
        ActionMapperComposition actionMapper,
        ITranslationService translationService,
        IErrorHandler errorHandler,
        RepositoryConfigurationReader repoConfigReader)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _actionMapper = actionMapper ?? throw new ArgumentNullException(nameof(actionMapper));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));

        _repoConfigReader = repoConfigReader ?? throw new ArgumentNullException(nameof(repoConfigReader));
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

        Dictionary<string, string> repositoryEnvVars = null;
        List<Variable> variables = null;
        List<ActionsCollection> actions = null;
        Exception ex = null;
        try
        {
            (repositoryEnvVars,  variables, actions, _) = _repoConfigReader.Get(repositories);
        }
        catch (Exception e)
        {
            ex = e;
        }

        if (ex != null)
        {
            if (ex is ConfigurationFileNotFoundException configurationFileNotFoundException)
            {
                foreach (RepositoryAction failingItem in CreateFailing(configurationFileNotFoundException, configurationFileNotFoundException.Filename))
                {
                    yield return failingItem;
                }
            }
            else
            {
                foreach (RepositoryAction failingItem in CreateFailing(ex, null))
                {
                    yield return failingItem;
                }
            }

            yield break;
        }

        using IDisposable d1 = RepoZVariableProviderStore.Push(EvaluateVariables(variables ?? new List<Variable>()));
        using IDisposable d2 = RepoZEnvironmentVariableStore.Set(repositoryEnvVars ?? new Dictionary<string, string>());

        // load variables global
        foreach (ActionsCollection actionsCollection in actions.Where(action => action != null))
        {
            using IDisposable d3 = RepoZVariableProviderStore.Push(EvaluateVariables(actionsCollection.Variables));

            foreach (Data.RepositoryAction action in actionsCollection.Actions)
            {
                using IDisposable d4 = RepoZVariableProviderStore.Push(EvaluateVariables(action.Variables));

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