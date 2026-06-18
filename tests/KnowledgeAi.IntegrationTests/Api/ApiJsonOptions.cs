using System.Text.Json;
using System.Text.Json.Serialization;

namespace KnowledgeAi.IntegrationTests.Api;

public static class ApiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}
