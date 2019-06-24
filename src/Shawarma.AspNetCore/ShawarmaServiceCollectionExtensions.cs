using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shawarma.AspNetCore.Internal;

namespace Shawarma.AspNetCore
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ShawarmaServiceCollectionExtensions
    {
        /// <summary>
        /// Register Shawarma with default options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddShawarma(this IServiceCollection services)
        {
            return services.AddShawarma(null);
        }

        /// <summary>
        /// Register Shawarma with custom options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="setupAction">Action to configure <see cref="ShawarmaOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddShawarma(this IServiceCollection services,
            Action<ShawarmaOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

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
