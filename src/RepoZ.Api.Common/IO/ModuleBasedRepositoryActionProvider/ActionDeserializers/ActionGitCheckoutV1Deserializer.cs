namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class ActionGitCheckoutV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "git-checkout@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer)
    {
        return Deserialize(jToken);
    }

    public RepositoryActionGitCheckoutV1 Deserialize(JToken jToken)
    {
        return jToken.ToObject<RepositoryActionGitCheckoutV1>();
    }
}