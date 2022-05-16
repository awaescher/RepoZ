namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoZ.Api.Git;
using RepoZ.Api.IO;
using RepositoryAction = RepoZ.Api.Git.RepositoryAction;

public class RepositorySpecificConfiguration
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly DynamicRepositoryActionDeserializer _appSettingsDeserializer;
    private readonly RepositoryExpressionEvaluator _repoExpressionEvaluator;

    public RepositorySpecificConfiguration(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        DynamicRepositoryActionDeserializer appsettingsDeserializer,
        RepositoryExpressionEvaluator repoExpressionEvaluator)
    {
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _appSettingsDeserializer = appsettingsDeserializer ?? throw new ArgumentNullException(nameof(appsettingsDeserializer));
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
    }

    public IEnumerable<RepositoryAction> Create(params RepoZ.Api.Git.Repository[] repository)
    {
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        Repository singleRepository = repository.SingleOrDefault();
        var multiSelectRequired = repository.Length > 1;

        // load default file
        RepositoryActionConfiguration2 rootFile = null;
        var filename = Path.Combine(_appDataPathProvider.GetAppDataPath(), "appsettings.json");
        if (!_fileSystem.File.Exists(filename))
        {
            throw new Exception("app settings file does not exists");
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
        if (!string.IsNullOrWhiteSpace(redirect?.Filename) && IsEnabled(redirect?.Enabled, true, singleRepository))
        {
            // load
            throw new NotImplementedException("todo, implement");
        }

        // select first one only, todo
        FileReference fileRef= rootFile.RepositorySpecificConfigFiles.FirstOrDefault(x => x != null);
        if (fileRef != null)
        {
            if (IsEnabled(fileRef.When, true, singleRepository))
            {
                var filenam1e = Evaluate(fileRef.Filename, repository.Single());
                if (_fileSystem.File.Exists(filenam1e))
                {
                    // try load 
                }
            }
        }


        if (rootFile.RepositorySpecificEnvironmentFiles?.Any() ?? false)
        {
            // load
            throw new NotImplementedException("todo, implement");
        }

        // load variables global

        if (rootFile.ActionsCollection != null)
        {
            // add variables to set
            // rootFile.ActionsCollection.Variables
            foreach (Data.RepositoryAction item in rootFile.ActionsCollection.Actions)
            {
                // add variables if any to set
                // item.Variables
                if ((!multiSelectRequired || IsEnabled(item.MultiSelectEnabled, false, singleRepository)) && IsEnabled(item.Active, true, singleRepository))
                {
                    yield return new RepositoryAction()
                        {
                            Action = null,
                            BeginGroup = false,
                            CanExecute = true,
                            Name = item.Name,
                            ExecutionCausesSynchronizing = true,
                            DeferredSubActionsEnumerator = null,
                        };
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