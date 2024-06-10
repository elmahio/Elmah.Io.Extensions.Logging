using Elmah.Io.Client;
using System;
using static Elmah.Io.Extensions.Logging.UserAgentHelper;

namespace Elmah.Io.Extensions.Logging
{
    internal class SynchronousMessageHandler(ElmahIoProviderOptions options) : ICanHandleMessages
    {
        private readonly ElmahIoProviderOptions _options = options;
        private IElmahioAPI _elmahIoClient;

        public void AddMessage(CreateMessage message)
        {
            if (_elmahIoClient == null)
            {
                var api = ElmahioAPI.Create(_options.ApiKey, new ElmahIoOptions
                {
                    WebProxy = _options.WebProxy,
                    Timeout = new TimeSpan(0, 0, 5),
                    UserAgent = UserAgent(),
                });
                api.Messages.OnMessageFail += (sender, args) => _options.OnError?.Invoke(args.Message, args.Error);
                _elmahIoClient = api;
            }

            _elmahIoClient.Messages.CreateAndNotify(_options.LogId, message);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
