using Elmah.Io.Client.Models;
using System;
using System.Net;

namespace Elmah.Io.Extensions.Logging
{
    public class ElmahIoProviderOptions
    {
        public string ApiKey { get; set; }
        public Guid LogId { get; set; }
        public Action<CreateMessage> OnMessage { get; set; }
        public Action<CreateMessage, Exception> OnError { get; set; }
        public IWebProxy WebProxy { get; set; }
    }
}
