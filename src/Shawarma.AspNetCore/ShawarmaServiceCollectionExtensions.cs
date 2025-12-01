using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shawarma.AspNetCore.Internal;

namespace Shawarma.AspNetCore;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>.
/// </summary>
public static class ShawarmaServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Register Shawarma with default options.
        /// </summary>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public IServiceCollection AddShawarma()
        {
            return services.AddShawarma(null);
        }

        /// <summary>
        /// Register Shawarma with custom options.
        /// </summary>
        /// <param name="setupAction">Action to configure <see cref="ShawarmaOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public IServiceCollection AddShawarma(Action<ShawarmaOptions>? setupAction)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<IShawarmaRequestHandler, ShawarmaRequestHandler>();
            services.TryAddSingleton<IApplicationStateProvider, ApplicationStateProvider>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}
