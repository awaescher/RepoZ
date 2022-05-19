namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class ActionExecutableV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "executable@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer)
    {
        return Deserialize(jToken);
    }

    public RepositoryActionExecutableV1 Deserialize(JToken jToken)
    {
        RepositoryActionExecutableV1 result = jToken.ToObject<RepositoryActionExecutableV1>();

        if (result == null)
        {
            return null;
        }

        if (result.Executables.Any())
        {
            return result;
        }

        JToken executableToken = jToken.SelectToken("executable");

        if (executableToken == null)
        {
            return result;
        }

        if (executableToken.Type != JTokenType.String)
        {
            return result;
        }

        var executable = executableToken.Value<string>();
        if (string.IsNullOrWhiteSpace(executable))
        {
            return result;
        }

        result.Executables = new List<string>(1)
            {
                executable,
            };

        return result;
    }
}