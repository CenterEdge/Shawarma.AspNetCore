using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Shawarma.AspNetCore.Hosting;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>.
/// </summary>
public static class ShawarmaHostingServiceCollectionExtensions
{
    /// <summary>
    /// Register Shawarma with <see cref="IShawarmaService"/> hosting support.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddShawarmaHosting(this IServiceCollection services)
    {
        services
            .AddShawarma()
            .AddHostedService<ShawarmaExecutor>();

        return services;
    }

    /// <summary>
    /// Register an <see cref="IShawarmaService"/>.
    /// </summary>
    /// <typeparam name="TShawarmaService">An implementation of <see cref="IShawarmaService"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddShawarmaService<
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        TShawarmaService>(this IServiceCollection services)
        where TShawarmaService : class, IShawarmaService
        => services.AddTransient<IShawarmaService, TShawarmaService>();
}
