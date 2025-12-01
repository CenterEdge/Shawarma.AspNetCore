using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shawarma.Internal;

internal sealed class ApplicationStatusConverter() : JsonStringEnumConverter<ApplicationStatus>(JsonNamingPolicy.CamelCase)
{
}
