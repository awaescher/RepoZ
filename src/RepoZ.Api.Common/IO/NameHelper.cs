namespace RepoZ.Api.Common.IO;

using System;
using System.Runtime.CompilerServices;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Git;

public static class NameHelper
{
    // todo

    public static string EvaluateName(in string input, in Repository repository, ITranslationService translationService, RepositoryExpressionEvaluator repositoryExpressionEvaluator)
    {
        return repositoryExpressionEvaluator.EvaluateStringExpression(
            ReplaceTranslatables(
                translationService.Translate(input),
                translationService),
            repository);
    }

    public static string ReplaceVariables(string value, Repository repository)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return Environment.ExpandEnvironmentVariables(
            value
                .Replace("{Repository.Name}", repository.Name)
                .Replace("{Repository.Path}", repository.Path)
                .Replace("{Repository.SafePath}", repository.SafePath)
                .Replace("{Repository.Location}", repository.Location)
                .Replace("{Repository.CurrentBranch}", repository.CurrentBranch)
                .Replace("{Repository.Branches}", string.Join("|", repository.Branches ?? Array.Empty<string>()))
                .Replace("{Repository.LocalBranches}", string.Join("|", repository.LocalBranches ?? Array.Empty<string>()))
                .Replace("{Repository.RemoteUrls}", string.Join("|", repository.RemoteUrls ?? Array.Empty<string>())));
    }

    private static string ReplaceTranslatables(string value, ITranslationService translationService)
    {
        if (value is null)
        {
            return string.Empty;
        }

        value = ReplaceTranslatable(value, "Open", translationService);
        value = ReplaceTranslatable(value, "OpenIn", translationService);
        value = ReplaceTranslatable(value, "OpenWith", translationService);

        return value;
    }

    private static string ReplaceTranslatable(string value, string translatable, ITranslationService translationService)
    {
        if (!value.StartsWith("{" + translatable + "}"))
        {
            return value;
        }

        var rest = value.Replace("{" + translatable + "}", "").Trim();
        return translationService.Translate("(" + translatable + ")", rest); // XMl doesn't support {}

    }
}