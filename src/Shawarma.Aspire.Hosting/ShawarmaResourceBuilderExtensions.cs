using System.Net.Http.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Shawarma.Aspire.Hosting.Internal;

namespace Shawarma.Aspire.Hosting;

/// <summary>
/// Extensions for adding Shawarma support to resource builders.
/// </summary>
public static class ShawarmaResourceBuilderExtensions
{
    private const string EnableShawarmaCommandName = "enableShawarma";
    private const string DisableShawarmaCommandName = "disableShawarma";

    private static readonly string[] s_httpSchemes = ["https", "http"];

    private static Func<EndpointReference> NamedEndpointSelector<TResource>(IResourceBuilder<TResource> builder, string endpointName)
        where TResource : IResourceWithEndpoints
        => () =>
        {
            // Find a matching endpoint using the name and if not an HTTP endpoint or not found throw an exception.
            var endpoints = builder.Resource.GetEndpoints();
            EndpointReference? matchingEndpoint = endpoints.FirstOrDefault(e => string.Equals(e.EndpointName, endpointName, StringComparison.OrdinalIgnoreCase));
            if (matchingEndpoint is not null)
            {
                if (!s_httpSchemes.Contains(matchingEndpoint.Scheme, StringComparer.OrdinalIgnoreCase))
                {
                    throw new DistributedApplicationException($"Could not create Shawarma command for resource '{builder.Resource.Name}' as the endpoint with name '{matchingEndpoint.EndpointName}' and scheme '{matchingEndpoint.Scheme}' is not an HTTP endpoint.");
                }
                return matchingEndpoint;
            }

            // No endpoint found with the specified name
            throw new DistributedApplicationException($"Could not create Shawarma command for resource '{builder.Resource.Name}' as no endpoint was found matching the specified name: {endpointName}");
        };

    /// <param name="builder">The <see cref="IResourceBuilder{T}"/>.</param>
    extension<T>(IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        /// <summary>
        /// Add commands to enable and disable Shawarma services for the resource.
        /// </summary>
        /// <param name="endpointName">The name of the HTTP endpoint on this resource to send the request to when the command is invoked.</param>
        /// <param name="autoStart">Automatically enable Shawarma services when the resource is ready.</param>
        /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
        public IResourceBuilder<T> WithShawarma([EndpointName] string? endpointName = null, bool autoStart = false) =>
            builder.WithShawarma(endpointName, new ShawarmaOptions()
            {
                AutoStart = autoStart
            });

        /// <summary>
        /// Add commands to enable and disable Shawarma services for the resource.
        /// </summary>
        /// <param name="endpointName">The name of the HTTP endpoint on this resource to send the request to when the command is invoked.</param>
        /// <param name="options">Optional configuration for Shawarma.</param>
        /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
        public IResourceBuilder<T> WithShawarma([EndpointName] string? endpointName = null, ShawarmaOptions? options = null) =>
            builder.WithShawarma(
                endpointSelector: endpointName is not null
                    ? NamedEndpointSelector(builder, endpointName)
                    : null,
                options: options);

        /// <summary>
        /// Add commands to enable and disable Shawarma services for the resource.
        /// </summary>
        /// <param name="endpointSelector">A callback that selects the HTTP endpoint to send the request to when the command is invoked.</param>
        /// <param name="options">Optional configuration for Shawarma.</param>
        /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
        public IResourceBuilder<T> WithShawarma(Func<EndpointReference>? endpointSelector, ShawarmaOptions? options = null)
        {
            options ??= ShawarmaOptions.Default;

            // Monitor the service to ensure it's running

            var targetRunning = false;
            builder.ApplicationBuilder.Eventing.Subscribe<BeforeStartEvent>((e, ct) =>
            {
                var resourceNotificationService = e.Services.GetRequiredService<ResourceNotificationService>();
                _ = Task.Run(async () =>
                {
                    await foreach (var resourceEvent in resourceNotificationService.WatchAsync(ct).WithCancellation(ct))
                    {
                        if (resourceEvent.Resource == (IResource)builder.Resource)
                        {
                            var resourceState = resourceEvent.Snapshot.State?.Text;
                            targetRunning = resourceState == KnownResourceStates.Running || resourceState == KnownResourceStates.RuntimeUnhealthy;
                        }
                    }
                }, ct);

                return Task.CompletedTask;
            });

            // Local function to handle the result of the HTTP commands
            async Task<ExecuteCommandResult> GetCommandResult(HttpCommandResultContext context)
            {
                if (context.Response.IsSuccessStatusCode)
                {
                    // Read the new state from the response
                    var newState = await context.Response.Content.ReadFromJsonAsync(
                            ShawarmaSerialization.ApplicationStateTypeInfo, cancellationToken: context.CancellationToken);

                    // Persist the state in the resource
                    var notificationService = context.ServiceProvider.GetRequiredService<ResourceNotificationService>();
                    await notificationService.PublishUpdateAsync(builder.Resource,
                        snapshot => snapshot.SetState(newState));

                    return CommandResults.Success();
                }
                else
                {
                    return CommandResults.Failure($"Failed to update Shawarma service state. Status code: {context.Response.StatusCode}.");
                }
            }

            builder = builder
                .WithHttpCommand(
                    path: options.ApplicationStatePath,
                    displayName: "Start Shawarma Services",
                    endpointSelector: endpointSelector,
                    commandName: EnableShawarmaCommandName,
                    commandOptions: new HttpCommandOptions
                    {
                        Method = HttpMethod.Post,
                        IconName = "Play",
                        HttpClientName = options.HttpClientName,
                        UpdateState = context =>
                        {
                            if (!targetRunning)
                            {
                                return ResourceCommandState.Disabled;
                            }

                            if (!context.ResourceSnapshot.TryGetState(out var state))
                            {
                                return ResourceCommandState.Enabled;
                            }

                            return state.Status == ApplicationStatus.Active
                                ? ResourceCommandState.Hidden
                                : ResourceCommandState.Enabled;
                        },
                        PrepareRequest = context =>
                        {
                            context.Request.Content = JsonContent.Create(new ApplicationState()
                            {
                                Status = ApplicationStatus.Active,
                                ActiveServices = ["default"]
                            }, ShawarmaSerialization.ApplicationStateTypeInfo);

                            return Task.CompletedTask;
                        },
                        GetCommandResult = GetCommandResult
                    })
                .WithHttpCommand(
                    path: options.ApplicationStatePath,
                    displayName: "Stop Shawarma Services",
                    endpointSelector: endpointSelector,
                    commandName: DisableShawarmaCommandName,
                    commandOptions: new HttpCommandOptions
                    {
                        Method = HttpMethod.Post,
                        IconName = "Stop",
                        HttpClientName = options.HttpClientName,
                        UpdateState = context =>
                        {
                            if (!targetRunning || !context.ResourceSnapshot.TryGetState(out var state))
                            {
                                return ResourceCommandState.Hidden;
                            }

                            return state.Status == ApplicationStatus.Active
                                ? ResourceCommandState.Enabled
                                : ResourceCommandState.Hidden;
                        },
                        PrepareRequest = context =>
                        {
                            context.Request.Content = JsonContent.Create(new ApplicationState()
                            {
                                Status = ApplicationStatus.Inactive,
                                ActiveServices = []
                            }, ShawarmaSerialization.ApplicationStateTypeInfo);

                            return Task.CompletedTask;
                        },
                        GetCommandResult = GetCommandResult
                    });

            // When starting, Shawarma will be disabled. Therefore, clear any previous status on restart.
            builder.OnBeforeResourceStarted(static async (resource, e, _) =>
            {
                var notificationService = e.Services.GetRequiredService<ResourceNotificationService>();
                await notificationService.PublishUpdateAsync(resource,
                    snapshot => snapshot.SetState(null));
            });

            if (options.AutoStart)
            {
                // Automatically enable Shawarma services when the resource is ready
                builder = builder.OnResourceReady(static async (resource, e, cancellationToken) =>
                {
                    var commandService = e.Services.GetRequiredService<ResourceCommandService>();

                    var result = await commandService.ExecuteCommandAsync(resource, EnableShawarmaCommandName, cancellationToken);
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
