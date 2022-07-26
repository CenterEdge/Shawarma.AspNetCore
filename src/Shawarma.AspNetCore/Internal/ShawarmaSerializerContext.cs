using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shawarma.AspNetCore.Internal
{
    [JsonSerializable(typeof(ApplicationState))]
    internal partial class ShawarmaSerializerContext : JsonSerializerContext
    {
        private static ShawarmaSerializerContext? _camelCase;

        public static ShawarmaSerializerContext Primary => _camelCase ??=
            new ShawarmaSerializerContext(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            });
    }
}
