#pragma warning disable IDE0130 // Namespace does not match folder structure
using System;

namespace Microsoft.AspNetCore.Builder
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DisableElmahIoFormLoggingAttribute : Attribute, IDisableElmahIoFormLoggingMetadata
    {
    }
}
