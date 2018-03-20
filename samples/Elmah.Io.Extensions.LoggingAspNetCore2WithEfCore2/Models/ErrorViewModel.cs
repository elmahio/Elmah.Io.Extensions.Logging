using System;

namespace Elmah.Io.Extensions.LoggingAspNetCore2WithEfCore2.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}