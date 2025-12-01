using System.Text.Json.Serialization.Metadata;

namespace Shawarma;

/// <summary>
/// Provides JSON serialization type information for Shawarma application state objects.
/// </summary>
public static class ShawarmaSerialization
{
    /// <summary>
    /// JSON serialization type info for <see cref="ApplicationState"/> objects.
    /// </summary>
    public static JsonTypeInfo<ApplicationState> ApplicationStateTypeInfo => ShawarmaSerializerContext.Default.ApplicationState;
}
