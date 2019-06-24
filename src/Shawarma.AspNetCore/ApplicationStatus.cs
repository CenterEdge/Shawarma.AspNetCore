using System;
using System.Runtime.Serialization;

namespace Shawarma.AspNetCore
{
    /// <summary>
    /// Status of the application.
    /// </summary>
    public enum ApplicationStatus
    {
        [EnumMember(Value = "active")]
        Active,

        [EnumMember(Value = "inactive")]
        Inactive
    }
}
