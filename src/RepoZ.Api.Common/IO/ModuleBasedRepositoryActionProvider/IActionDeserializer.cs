namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using Newtonsoft.Json.Linq;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

public interface IActionDeserializer
{
    bool CanDeserialize(string type);

    RepositoryAction Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer);
}