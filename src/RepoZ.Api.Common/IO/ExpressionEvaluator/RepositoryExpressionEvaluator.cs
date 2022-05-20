namespace RepoZ.Api.Common.IO.ExpressionEvaluator;

using ExpressionStringEvaluator.Methods.BooleanToBoolean;
using ExpressionStringEvaluator.Methods.Flow;
using ExpressionStringEvaluator.Methods.StringToBoolean;
using ExpressionStringEvaluator.Methods.StringToInt;
using ExpressionStringEvaluator.Methods.StringToString;
using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.Parser;
using ExpressionStringEvaluator.VariableProviders.DateTime;
using ExpressionStringEvaluator.VariableProviders;
using System.Collections.Generic;
using System.IO.Abstractions;
using System;
using System.Linq;
using System.Threading;
using RepoZ.Api.Git;

public class RepositoryContext
{
    public RepositoryContext()
    {
        Repositories = Array.Empty<Repository>();
    }

    public RepositoryContext(params Repository[] repositories)
    {
        Repositories = repositories.ToArray();
    }

    public RepositoryContext(IEnumerable<Repository> repositories)
    {
        Repositories = repositories.ToArray();
    }

    public Repository[] Repositories { get; }
}

public class RepositoryExpressionEvaluator
{
    private readonly ExpressionExecutor _expressionExecutor;

    public RepositoryExpressionEvaluator(IEnumerable<IVariableProvider> variableProviders, IEnumerable<IMethod> methods)
    {
        List<IVariableProvider> v = variableProviders?.ToList() ?? throw new ArgumentNullException(nameof(variableProviders));
        List<IMethod> m = methods?.ToList() ?? throw new ArgumentNullException(nameof(methods));

        _expressionExecutor = new ExpressionStringEvaluator.Parser.ExpressionExecutor(v, m);
    }

    public string EvaluateStringExpression(string value, params Repository[] repository)
    {
        return EvaluateStringExpression(value, repository.AsEnumerable());
    }

    public string EvaluateStringExpression(string value, IEnumerable<Repository> repository)
    {
        if (value == null)
        {
            return string.Empty;
        }

        try
        {
            CombinedTypeContainer result = _expressionExecutor.Execute<RepositoryContext>(new RepositoryContext(repository), value);
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

    public bool EvaluateBooleanExpression(string value, Repository repository)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        try
        {
            CombinedTypeContainer result = _expressionExecutor.Execute<RepositoryContext>(new RepositoryContext(repository), value);
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
}