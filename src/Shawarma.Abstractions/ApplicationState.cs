using System;

// ReSharper disable once CheckNamespace
namespace Shawarma
{
    /// <summary>
    /// Represents the current application state as provided by Shawarma via the endpoint.
    /// </summary>
    public class ApplicationState
    {
        /// <summary>
        /// Status of the application.
        /// </summary>
        public ApplicationStatus Status { get; set; }
    }
}
