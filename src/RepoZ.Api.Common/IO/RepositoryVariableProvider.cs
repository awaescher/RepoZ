namespace RepoZ.Api.Common.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionStringEvaluator.VariableProviders;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Git;

public class RepositoryVariableProvider : IVariableProvider<RepositoryContext>
{
    public bool CanProvide(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && key.StartsWith("Repository.", StringComparison.CurrentCultureIgnoreCase);
    }

    public string Provide(RepositoryContext context, string key, string arg)
    {
        Repository repository = context?.Repositories.SingleOrDefault();
        if (repository == null)
        {
            return string.Empty;
        }

        var startIndex = "Repository.".Length;
        var keySuffix = key.Substring(startIndex, key.Length - startIndex);

        if ("Name".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.Name;
        }

        if ("Path".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.Path;
        }

        if ("SafePath".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.SafePath;
        }

        if ("Location".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.Location;
        }

        if ("CurrentBranch".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.CurrentBranch;
        }

        if ("Branches".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", repository.Branches);
        }

        if ("LocalBranches".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", repository.LocalBranches);
        }

        if ("RemoteUrls".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", repository.RemoteUrls);
        }

        throw new NotImplementedException();
    }

    public string Provide(string key, string arg)
    {
        throw new NotImplementedException();
    }
}