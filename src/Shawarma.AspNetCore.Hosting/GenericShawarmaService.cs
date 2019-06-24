using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Shawarma.AspNetCore.Hosting
{
    /// <summary>
    /// Abstract implementation for a basic <see cref="IShawarmaService"/>.
    /// </summary>
    public abstract class GenericShawarmaService : IShawarmaService
    {
        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <summary>
        /// An <see cref="ILogger"/> for writing logs.
        /// </summary>
        protected  ILogger Logger { get; }

        /// <summary>
        /// Creates a new <see cref="GenericShawarmaService"/>.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> used for logging. This should normally be a generic <see cref="ILogger{TCategoryName}"/> based on the service's type.</param>
        protected GenericShawarmaService(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task UpdateStateAsync(ApplicationState state, CancellationToken cancellationToken)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var desiredRunState = GetDesiredRunState(state);

            if (desiredRunState && !IsRunning)
            {
                Logger.LogInformation("Starting service");
                IsRunning = true;

                return StartInternalAsync(cancellationToken);
            }

            if (!desiredRunState)
            {
                return StopAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (IsRunning)
            {
                Logger.LogInformation("Stopping service");
                IsRunning = false;

                return StopInternalAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines the desired run state based on <see cref="ApplicationState"/>.
        /// By default, returns true if <see cref="ApplicationState.Status"/> is <see cref="ApplicationStatus.Active"/>.
        /// </summary>
        /// <param name="state">The <see cref="ApplicationState"/>.</param>
        /// <returns>True if the desired state is running, false if stopped.</returns>
        protected virtual bool GetDesiredRunState(ApplicationState state)
        {
            return state.Status == ApplicationStatus.Active;
        }

        /// <summary>
        /// Overriden by a derived class to start the service.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> triggered to give up on starting.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        protected abstract Task StartInternalAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Overriden by a derived class to stop the service.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> triggered to give up on stopping.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        protected abstract Task StopInternalAsync(CancellationToken cancellationToken);
    }
}
