namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

public class RepositorySpecificConfiguration
{
    public RepositorySpecificConfiguration()
    {
    }

    public RepositoryActionConfiguration2 Create(RepoZ.Api.Git.Repository repository)
    {
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        return null;

        // get the root,
        // redirect, redirect, redirect

        // get the includes env
        // get the includes repo specific
        //  (redirect, redirect, redirect)
    }
}