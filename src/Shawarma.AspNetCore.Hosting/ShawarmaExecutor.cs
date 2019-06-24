using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Shawarma.AspNetCore.Hosting
{
    /// <summary>
    /// <see cref="IHostedService"/> that manages <see cref="IShawarmaService"/>
    /// </summary>
    public class ShawarmaExecutor : IHostedService
    {
        private readonly IApplicationStateProvider _stateProvider;
        private readonly IEnumerable<IShawarmaService> _services;
        private readonly ILogger<ShawarmaExecutor> _logger;

        private IDisposable _changeHandler;

        /// <summary>
        /// Creates a new <see cref="ShawarmaExecutor"/>.
        /// </summary>
        /// <param name="stateProvider">The <see cref="IApplicationStateProvider"/>.</param>
        /// <param name="services">List of <see cref="IShawarmaService"/> instances to manage.</param>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}"/> for logging.</param>
        public ShawarmaExecutor(IApplicationStateProvider stateProvider,
            IEnumerable<IShawarmaService> services,
            ILogger<ShawarmaExecutor> logger)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _changeHandler = ChangeToken.OnChange(
                () => _stateProvider.GetChangeToken(),
                HandleStateChange);

            var state = await _stateProvider.GetApplicationStateAsync();
            await ExecuteAsync(service => service.UpdateStateAsync(state, cancellationToken));
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _changeHandler?.Dispose();

            return ExecuteAsync(service => service.StopAsync(cancellationToken), throwOnFirstFailure: false);
        }

        private void HandleStateChange()
        {
            Task.Run(async () =>
            {
                try
                {
                    var state = await _stateProvider.GetApplicationStateAsync();
                    _logger.LogInformation("Handling Shawarma State Change: {status}", state.Status);
                    await ExecuteAsync(service => service.UpdateStateAsync(state, CancellationToken.None));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling Shawarma state change");
                }
            });
        }

        // Note: Borrowed from ASP.NET Core HostedServiceExecutor
        private async Task ExecuteAsync(Func<IShawarmaService, Task> callback, bool throwOnFirstFailure = true)
        {
            List<Exception> exceptions = null;

            foreach (var service in _services)
            {
                try
                {
                    await callback(service);
                }
                catch (Exception ex)
                {
                    if (throwOnFirstFailure)
                    {
                        throw;
                    }

                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            // Throw an aggregate exception if there were any exceptions
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
