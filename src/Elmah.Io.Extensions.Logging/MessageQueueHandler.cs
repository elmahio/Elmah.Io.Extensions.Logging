using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elmah.Io.Client;
using static Elmah.Io.Extensions.Logging.UserAgentHelper;

namespace Elmah.Io.Extensions.Logging
{
    internal class MessageQueueHandler(ElmahIoProviderOptions options) : ICanHandleMessages
    {
        private readonly ElmahIoProviderOptions options = options;
        private IElmahioAPI elmahIoClient;
        private BlockingCollection<CreateMessage> messages;
        private CancellationTokenSource cancellationTokenSource;
        private readonly List<CreateMessage> currentBatch = [];
        private int messagesDropped;

        internal MessageQueueHandler(ElmahIoProviderOptions options, IElmahioAPI elmahIoClient) : this(options)
        {
            this.elmahIoClient = elmahIoClient;
        }

        public void Start()
        {
            messages = new BlockingCollection<CreateMessage>(new ConcurrentQueue<CreateMessage>(), options.BackgroundQueueSize);
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(ProcessLogQueue);
        }

        public void Stop()
        {
            try
            {
                if (messages.Count > 0)
                {
                    // Remaining messages in queue. Flush them if possible.
                    ProcessMessages().GetAwaiter().GetResult();
                }
            }
            catch (Exception e) when (e is TaskCanceledException || (e is AggregateException ae && ae.InnerExceptions.Count == 1 && ae.InnerExceptions[0] is TaskCanceledException))
            {
                // If a TaskCanceledException is thrown while stopping there is not much to do
            }

            cancellationTokenSource.Cancel();
            messages.CompleteAdding();
        }

        public void AddMessage(CreateMessage message)
        {
            if (!messages.IsAddingCompleted)
            {
                try
                {
                    if (!messages.TryAdd(message, millisecondsTimeout: 0, cancellationToken: cancellationTokenSource.Token))
                    {
                        Interlocked.Increment(ref messagesDropped);
                    }
                }
                catch
                {
                    // Cancellation token canceled or CompleteAdding called
                }
            }
        }

        private async Task WriteMessagesAsync(IEnumerable<CreateMessage> messages, CancellationToken token)
        {
            if (elmahIoClient == null)
            {
                var api = ElmahioAPI.Create(options.ApiKey, new ElmahIoOptions
                {
                    WebProxy = options.WebProxy,
                    Timeout = new TimeSpan(0, 0, 30),
                    UserAgent = UserAgent(),
                });
                api.Messages.OnMessageFail += (sender, args) => options.OnError?.Invoke(args.Message, args.Error);
                elmahIoClient = api;
            }

            var bulk = new List<CreateMessage>();
            foreach (var message in messages)
            {
                bulk.Add(message);

                if (bulk.Count >= options.BatchPostingLimit)
                {
                    await elmahIoClient.Messages.CreateBulkAndNotifyAsync(options.LogId, bulk, token);
                    bulk.Clear();
                }
            }

            if (bulk.Count > 0)
            {
                await elmahIoClient.Messages.CreateBulkAndNotifyAsync(options.LogId, bulk, token);
            }

        }

        private static Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            return Task.Delay(interval, cancellationToken);
        }

        private async Task ProcessLogQueue()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                // Process messages
                await ProcessMessages();
                // Then wait until next check
                await IntervalAsync(options.Period, cancellationTokenSource.Token);
            }
        }

        private async Task ProcessMessages()
        {
            while (messages.TryTake(out var message))
            {
                currentBatch.Add(message);
            }

            var messagesDroppedExchange = Interlocked.Exchange(ref this.messagesDropped, 0);
            if (messagesDroppedExchange != 0)
            {
                currentBatch.Add(new CreateMessage { Title = $"{messagesDroppedExchange} message(s) dropped because of queue size limit. Increase the queue size or decrease logging verbosity to avoid this." });
            }

            if (currentBatch.Count > 0)
            {
                try
                {
                    await WriteMessagesAsync(currentBatch, cancellationTokenSource.Token);
                }
                catch
                {
                    // ignored
                }

                currentBatch.Clear();
            }
        }
    }
}
