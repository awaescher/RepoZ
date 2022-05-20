namespace RepoZ.Api.Common.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoZ.Api.Git;

public static class MultipleRepositoryActionHelper
{
    public static RepositoryAction CreateActionForMultipleRepositories(
        string name,
        IEnumerable<Repository> repositories,
        Action<Repository> action,
        bool executionCausesSynchronizing = false)
    {
        return new RepositoryAction()
            {
                Name = name,
                Action = (_, _) =>
                    {
                        // copy over to an array to not get an exception
                        // once the enumerator changes (which can happen when a change
                        // is detected and a repository is renewed) while the loop is running
                        Repository[] repositoryArray = repositories.ToArray();

                        foreach (Repository repository in repositoryArray)
                        {
                            SafelyExecute(action, repository); // git/io-exceptions will break the loop, put in try/catch
                        }
                    },
                ExecutionCausesSynchronizing = executionCausesSynchronizing,
            };
    }

    private static void SafelyExecute(Action<Repository> action, Repository repository)
    {
        try
        {
            action(repository);
        }
        catch
        {
            // nothing to see here
        }
    }
}