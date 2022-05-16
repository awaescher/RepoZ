namespace RepoZ.Api.Common.Tests.TestFramework;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using VerifyTests;

public static class VerifierInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DisableRequireUniquePrefix();
        VerifierSettings.AddExtraSettings(serializerSettings => serializerSettings.TypeNameHandling = TypeNameHandling.Auto);
    }
}