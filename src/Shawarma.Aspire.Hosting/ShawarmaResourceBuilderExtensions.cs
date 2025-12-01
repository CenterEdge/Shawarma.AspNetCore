using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Shawarma.Aspire.Hosting.Internal;

namespace Shawarma.Aspire.Hosting;

/// <summary>
/// Extensions for adding Shawarma support to resource builders.
/// </summary>
public static class ShawarmaResourceBuilderExtensions
{
    private const string DefaultEndpointName = "http";

    /// <param name="builder">The <see cref="IResourceBuilder{T}"/>.</param>
    extension<T>(IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        /// <summary>
        /// Add commands to enable and disable Shawarma services for the resource.
        /// </summary>
        /// <param name="endpointName">Name of the endpoint used to activate services, defaults to "http".</param>
        /// <param name="autoStart">Automatically enable Shawarma services when the resource is ready.</param>
        /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
        public IResourceBuilder<T> WithShawarma(string? endpointName = null, bool autoStart = false)
        {
            endpointName ??= DefaultEndpointName;

            builder = builder
                .WithCommand("shawarmaEnable", "Start Shawarma Services",
                    context => ShawarmaStateHandler.SetShawarmaStatusAsync(context.ServiceProvider, builder.Resource, endpointName, ApplicationStatus.Active, context.CancellationToken),
                    new CommandOptions()
                    {
                        IconName = "Play",
                        UpdateState = context =>
                        {
                            if (!context.ResourceSnapshot.TryGetState(out var state))
                            {
                                return ResourceCommandState.Enabled;
                            }

                            return state.Status == ApplicationStatus.Active ? ResourceCommandState.Hidden : ResourceCommandState.Enabled;
                        }
                    })
                .WithCommand("shawarmaDisable", "Stop Shawarma Services",
                    context => ShawarmaStateHandler.SetShawarmaStatusAsync(context.ServiceProvider, builder.Resource, endpointName, ApplicationStatus.Inactive, context.CancellationToken),
                    new CommandOptions()
                    {
                        IconName = "Stop",
                        UpdateState = context =>
                        {
                            if (!context.ResourceSnapshot.TryGetState(out var state))
                            {
                                return ResourceCommandState.Hidden;
                            }

                            return state.Status == ApplicationStatus.Active ? ResourceCommandState.Enabled : ResourceCommandState.Hidden;
                        }
                    });

            if (autoStart)
            {
                builder = builder.OnResourceReady(async (resource, e, cancellationToken) =>
                {
                    var result = await ShawarmaStateHandler.SetShawarmaStatusAsync(e.Services, resource, endpointName, ApplicationStatus.Active, cancellationToken);
                    if (result is { Success: false, Canceled: false })
                    {
                        throw new InvalidOperationException($"Failed to enable Shawarma services on startup: {result.ErrorMessage}");
                    }
                });
            }

            return builder;
        }
    }
}
