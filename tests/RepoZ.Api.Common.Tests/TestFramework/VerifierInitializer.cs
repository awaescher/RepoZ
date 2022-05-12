namespace RepoZ.Api.Common.Tests.TestFramework;

using System.Runtime.CompilerServices;
using VerifyTests;

public static class VerifierInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DisableRequireUniquePrefix();
    }
}