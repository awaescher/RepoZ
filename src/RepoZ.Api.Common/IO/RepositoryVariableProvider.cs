namespace RepoZ.Api.Common.IO;

using System;
using ExpressionStringEvaluator.VariableProviders;
using RepoZ.Api.Git;

public class RepositoryVariableProvider : IVariableProvider<Repository>
{
    public bool CanProvide(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && key.StartsWith("Repository.", StringComparison.CurrentCultureIgnoreCase);
    }

    public string Provide(Repository context, string key, string arg)
    {
        var startIndex = "Repository.".Length;
        var keySuffix = key.Substring(startIndex, key.Length - startIndex);


        if ("Name".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return context.Name;
        }

        if ("Path".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return context.Path;
        }

        if ("SafePath".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return context.SafePath;
        }

        if ("Location".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return context.Location;
        }

        if ("CurrentBranch".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return context.CurrentBranch;
        }

        if ("Branches".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", context.Branches);
        }

        if ("LocalBranches".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", context.LocalBranches);
        }

        if ("RemoteUrls".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", context.RemoteUrls);
        }

        throw new NotImplementedException();
    }

    public string Provide(string key, string arg)
    {
        throw new NotImplementedException();
    }
}