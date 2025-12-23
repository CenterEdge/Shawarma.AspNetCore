namespace Shawarma.Aspire.Hosting;

public class ShawarmaOptions
{
    internal static ShawarmaOptions Default { get; } = new();

    /// <summary>
    /// Automatically enable Shawarma services when the resource is ready.
    /// </summary>
    public bool AutoStart { get; set; }

    /// <summary>
    /// Gets or sets the name of the HTTP client to use when creating it via <see cref="IHttpClientFactory.CreateClient(string)"/>.
    /// </summary>
    public string? HttpClientName { get; set; }

    /// <summary>
    /// Path to the application state endpoint.
    /// </summary>
    public string ApplicationStatePath
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            field = value;
        }
    } = "/applicationstate";
}
