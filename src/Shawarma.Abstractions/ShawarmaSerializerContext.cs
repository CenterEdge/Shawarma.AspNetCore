using System.Text.Json.Serialization;
using Shawarma.Internal;

namespace Shawarma;

[JsonSerializable(typeof(ApplicationState))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(ApplicationStatusConverter)])]
internal sealed partial class ShawarmaSerializerContext : JsonSerializerContext
{
}
