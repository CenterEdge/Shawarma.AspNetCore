using System;
using System.Collections.Generic;

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

        /// <summary>
        /// List of services which are active.
        /// </summary>
        /// <remarks>
        /// Requires Shawarma 1.1.
        /// </remarks>
        public List<string> ActiveServices { get; set; } = new();
    }
}
