using System;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Shawarma
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
