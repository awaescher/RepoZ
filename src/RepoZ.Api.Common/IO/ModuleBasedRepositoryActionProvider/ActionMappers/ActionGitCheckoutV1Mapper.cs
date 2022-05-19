namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoZ.Api.Git;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionGitCheckoutV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryWriter _repositoryWriter;
    private readonly IErrorHandler _errorHandler;

    public ActionGitCheckoutV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IRepositoryWriter repositoryWriter, IErrorHandler errorHandler)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionGitCheckoutV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipeRepositories()
    {
        return false;
    }

    IEnumerable<Api.Git.RepositoryAction> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionGitCheckoutV1, repository.First());
    }

    public IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionGitCheckoutV1 action, Repository repository)
    {
        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = action.Name;
        if (!string.IsNullOrEmpty(name))
        {
            name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = _translationService.Translate("Checkout");
        }

        yield return new Api.Git.RepositoryAction()
            {
                Name = _translationService.Translate("Checkout"),
                DeferredSubActionsEnumerator = () =>
                    repository.LocalBranches
                              .Take(50)
                              .Select(branch => new Api.Git.RepositoryAction()
                                  {
                                      Name = branch,
                                      Action = (_, _) => _repositoryWriter.Checkout(repository, branch),
                                      CanExecute = !repository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                  })
                              .Union(new[]
                                  {
                                      new RepositorySeparatorAction(), // doesn't work todo
                                      new Api.Git.RepositoryAction()
                                          {
                                              Name = _translationService.Translate("Remote branches"),
                                              DeferredSubActionsEnumerator = () =>
                                                  {
                                                      Api.Git.RepositoryAction[] remoteBranches = repository.ReadAllBranches()
                                                                                                            .Select(branch => new Api.Git.RepositoryAction()
                                                                                                                {
                                                                                                                    Name = branch,
                                                                                                                    Action = (_, _) => _repositoryWriter.Checkout(repository, branch),
                                                                                                                    CanExecute = !repository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                                                                                                })
                                                                                                            .ToArray();

                                                      if (remoteBranches.Any())
                                                      {
                                                          return remoteBranches;
                                                      }

                                                      return new Api.Git.RepositoryAction[]
                                                          {
                                                              new Api.Git.RepositoryAction()
                                                                  {
                                                                      Name = _translationService.Translate("No remote branches found"),
                                                                      CanExecute = false,
                                                                  },
                                                              new Api.Git.RepositoryAction()
                                                                  {
                                                                      Name = _translationService.Translate("Try to fetch changes if you're expecting remote branches"),
                                                                      CanExecute = false,
                                                                  },
                                                          };
                                                  },
                                          },
                                  })
                              .ToArray(),
            };
    }
}