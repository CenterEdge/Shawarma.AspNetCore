using System.Net.Http.Json;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace Shawarma.Aspire.Hosting.Internal;

internal static class ShawarmaStateHandler
{
    private static readonly HttpClient HttpClient = new(new SocketsHttpHandler()
    {
        PooledConnectionIdleTimeout = TimeSpan.FromSeconds(5),
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    });

    extension(EndpointReference endpoint)
    {
        private Uri ApplicationStateUri =>
            new Uri(new Uri(endpoint.Url), "applicationstate");
    }

    public static async Task<ExecuteCommandResult> SetShawarmaStatusAsync(IServiceProvider serviceProvider, IResourceWithEndpoints resource, string endpointName, ApplicationStatus status, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentException.ThrowIfNullOrEmpty(endpointName);

        try
        {
            var newState = new ApplicationState()
            {
                Status = status,
                ActiveServices = status == ApplicationStatus.Active ? ["default"] : []
            };

            var endpoint = resource.GetEndpoints()
                .FirstOrDefault(endpoint => endpoint.EndpointName == endpointName);
            if (endpoint is null)
            {
                return new ExecuteCommandResult() { Success = false, ErrorMessage = $"Endpoint '{endpointName}' not found." };
            }

            using var response = await HttpClient.PostAsJsonAsync(
                endpoint.ApplicationStateUri,
                newState,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // Read the true new state from the response
                newState = await response.Content.ReadFromJsonAsync<ApplicationState>(ShawarmaSerialization.ApplicationStateTypeInfo, cancellationToken: cancellationToken);

                // Persist the state in the resource
                var notificationService = serviceProvider.GetRequiredService<ResourceNotificationService>();
                await notificationService.PublishUpdateAsync(resource,
                    snapshot => snapshot.SetState(newState));

                return new ExecuteCommandResult() { Success = true };
            }
            else
            {
                return new ExecuteCommandResult() { Success = false, ErrorMessage = $"Failed to set Shawarma services to '{status}'. Status code: {response.StatusCode}." };
            }
        }
        catch (OperationCanceledException)
        {
            return new ExecuteCommandResult { Success = false, Canceled = true };
        }
        catch (Exception ex)
        {
            return new ExecuteCommandResult() { Success = false, ErrorMessage = $"Exception occurred while setting Shawarma services to '{status}': {ex.Message}" };
        }
    }
}
