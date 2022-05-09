namespace RepoZ.Api.Common.IO;

using System;
using System.Collections.Generic;
using System.IO;
using DotNetEnv;
using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.Methods.BooleanToBoolean;
using ExpressionStringEvaluator.Methods.Flow;
using ExpressionStringEvaluator.Methods.StringToBoolean;
using ExpressionStringEvaluator.Methods.StringToInt;
using ExpressionStringEvaluator.Methods.StringToString;
using ExpressionStringEvaluator.Parser;
using ExpressionStringEvaluator.VariableProviders;
using ExpressionStringEvaluator.VariableProviders.DateTime;
using RepoZ.Api.Git;

public static class RepositoryExpressionEvaluator
{
    private static readonly Dictionary<string, string> _emptyDictionary = new Dictionary<string, string>(0);
    private static readonly ExpressionExecutor _expressionExecutor;

    static RepositoryExpressionEvaluator()
    {
        var dateTimeTimeVariableProviderOptions = new DateTimeVariableProviderOptions()
            {
                DateTimeProvider = () => DateTime.Now,
            };

        var dateTimeNowVariableProviderOptions = new DateTimeNowVariableProviderOptions()
            {
                DateTimeProvider = () => DateTime.Now,
            };

        var dateTimeDateVariableProviderOptions = new DateTimeDateVariableProviderOptions()
            {
                DateTimeProvider = () => DateTime.Now,
            };

        var providers = new List<IVariableProvider>
            {
                new DateTimeNowVariableProvider(dateTimeNowVariableProviderOptions),
                new DateTimeTimeVariableProvider(dateTimeTimeVariableProviderOptions),
                new DateTimeDateVariableProvider(dateTimeDateVariableProviderOptions),
                new EmptyVariableProvider(),
                new CustomEnvironmentVariableVariableProvider(GetRepoEnvironmentVariables),
                new RepositoryVariableProvider(),
                new SlashVariableProvider(),
                new BackslashVariableProvider(),
            };

        var methods = new List<IMethod>
            {
                new StringTrimEndStringMethod(),
                new StringTrimStartStringMethod(),
                new StringTrimStringMethod(),
                new StringContainsStringMethod(),
                new StringLowerStringMethod(),
                new StringUpperStringMethod(),
                new UrlEncodeStringMethod(),
                new UrlDecodeStringMethod(),
                new StringEqualsStringMethod(),
                new AndBooleanMethod(),
                new OrBooleanMethod(),
                new StringIsNullOrEmptyBooleanMethod(),
                new FileExistsBooleanMethod(),
                new NotBooleanMethod(),
                new StringLengthMethod(),
                new IfThenElseMethod(),
                new IfThenMethod(),
                new InMethod(),
                new StringReplaceMethod(),
                new SubstringMethod(),
            };

        _expressionExecutor = new ExpressionStringEvaluator.Parser.ExpressionExecutor(providers, methods);
    }

    public static string EvaluateStringExpression(string value, Repository repository)
    {
        if (value == null)
        {
            return string.Empty;
        }

        try
        {
            CombinedTypeContainer result = RepositoryExpressionEvaluator._expressionExecutor.Execute<Repository>(repository, value);
            if (result.IsString(out var s))
            {
                return s;
            }

            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static bool EvaluateBooleanExpression(string value, Repository repository)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        try
        {
            CombinedTypeContainer result = RepositoryExpressionEvaluator._expressionExecutor.Execute<Repository>(repository, value);
            if (result.IsBool(out var b))
            {
                return b.Value;
            }

            if ("true".Equals(result.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }


    private static Dictionary<string, string> GetRepoEnvironmentVariables(Repository repository)
    {
        var repozEnvFile = Path.Combine(repository.Path, ".git", "repoz.env");

        if (!File.Exists(repozEnvFile))
        {
            return _emptyDictionary;
        }

        try
        {
            return DotNetEnv.Env.Load(repozEnvFile, new DotNetEnv.LoadOptions(setEnvVars: false)).ToDictionary();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return _emptyDictionary;
    }
}