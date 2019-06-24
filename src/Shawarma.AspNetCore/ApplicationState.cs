using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Shawarma.AspNetCore
{
    /// <summary>
    /// Represents the current application state as provided by Shawarma via the endpoint.
    /// </summary>
    public class ApplicationState
    {
        /// <summary>
        /// Status of the application.
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApplicationStatus Status { get; set; }
    }
}
