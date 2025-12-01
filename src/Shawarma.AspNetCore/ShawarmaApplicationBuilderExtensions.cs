using Microsoft.AspNetCore.Builder;
using Shawarma.AspNetCore.Internal;

namespace Shawarma.AspNetCore;

/// <summary>
/// Extensions for <see cref="IApplicationBuilder"/>.
/// </summary>
public static class ShawarmaApplicationBuilderExtensions
{
    /// <summary>
    /// Add <see cref="ShawarmaMiddleware"/> to the stack, using the default URL.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
    /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
    [Obsolete(ShawarmaConstants.UseEndpointRoutingWarning)]
    public static IApplicationBuilder UseShawarma(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<ShawarmaMiddleware>();
    }

    /// <summary>
    /// Add <see cref="ShawarmaMiddleware"/> to the stack, using the default URL.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
    /// <param name="setupAction">Action to configure options.</param>
    /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
    [Obsolete(ShawarmaConstants.UseEndpointRoutingWarning)]
    public static IApplicationBuilder UseShawarma(this IApplicationBuilder app, Action<ShawarmaOptions>? setupAction)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (setupAction == null)
        {
            // Don't pass options so it can be configured/injected via DI container instead
            app.UseMiddleware<ShawarmaMiddleware>();
        }
        else
        {
            // Configure an options instance here and pass directly to the middleware
            var options = new ShawarmaOptions();
            setupAction.Invoke(options);

            app.UseMiddleware<ShawarmaMiddleware>(options);
        }

        return app;
    }
}
