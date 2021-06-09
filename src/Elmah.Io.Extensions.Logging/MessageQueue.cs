using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Elmah.Io.Client;

namespace Elmah.Io.Extensions.Logging
{
    internal class MessageQueue : ICanHandleMessages
    {
        internal static string _assemblyVersion = typeof(ElmahIoLogger).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        private readonly ElmahIoProviderOptions _options;
        private IElmahioAPI _elmahIoClient;
        private BlockingCollection<CreateMessage> _messages;
        private Task _outputTask;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly List<CreateMessage> _currentBatch = new List<CreateMessage>();
        private int _messagesDropped;

        internal MessageQueue(ElmahIoProviderOptions options, IElmahioAPI elmahIoClient) : this(options)
        {
            _elmahIoClient = elmahIoClient;
        }

        public MessageQueue(ElmahIoProviderOptions options)
        {
            this._options = options;
        }

        internal void Start()
        {
            _messages = new BlockingCollection<CreateMessage>(new ConcurrentQueue<CreateMessage>(), _options.BackgroundQueueSize);
            _cancellationTokenSource = new CancellationTokenSource();
            _outputTask = Task.Run(ProcessLogQueue);
        }

        internal void Stop()
        {
            try
            {
                if (_messages.Count > 0)
                {
                    // Remaining messages in queue. Flush them if possible.
                    ProcessMessages().GetAwaiter().GetResult();
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            {
            }

            _cancellationTokenSource.Cancel();
            _messages.CompleteAdding();
        }

        public void AddMessage(CreateMessage message)
        {
            if (!_messages.IsAddingCompleted)
            {
                try
                {
                    if (!_messages.TryAdd(message, millisecondsTimeout: 0, cancellationToken: _cancellationTokenSource.Token))
                    {
                        Interlocked.Increment(ref _messagesDropped);
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
            if (_elmahIoClient == null)
            {
                var api = (ElmahioAPI)ElmahioAPI.Create(_options.ApiKey, new ElmahIoOptions
                {
                    WebProxy = _options.WebProxy
                });
                api.HttpClient.Timeout = new TimeSpan(0, 0, 30);
                api.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Elmah.Io.Extensions.Logging", _assemblyVersion)));
                api.Messages.OnMessageFail += (sender, args) => _options.OnError?.Invoke(args.Message, args.Error);
                _elmahIoClient = api;
            }

            var bulk = new List<CreateMessage>();
            foreach (var message in messages)
            {
                bulk.Add(message);

                if (bulk.Count >= _options.BatchPostingLimit)
                {
                    await _elmahIoClient.Messages.CreateBulkAndNotifyAsync(_options.LogId, bulk);
                    bulk.Clear();
                }
            }

            if (bulk.Count > 0)
            {
                await _elmahIoClient.Messages.CreateBulkAndNotifyAsync(_options.LogId, bulk);
            }

        }

        private Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            return Task.Delay(interval, cancellationToken);
        }

        private async Task ProcessLogQueue()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                // Process messages
                await ProcessMessages();
                // Then wait until next check
                await IntervalAsync(_options.Period, _cancellationTokenSource.Token);
            }
        }

        private async Task ProcessMessages()
        {
            while (_messages.TryTake(out var message))
            {
                _currentBatch.Add(message);
            }

            var messagesDropped = Interlocked.Exchange(ref _messagesDropped, 0);
            if (messagesDropped != 0)
            {
                _currentBatch.Add(new CreateMessage { Title = $"{messagesDropped} message(s) dropped because of queue size limit. Increase the queue size or decrease logging verbosity to avoid this." });
            }

            if (_currentBatch.Count > 0)
            {
                try
                {
                    await WriteMessagesAsync(_currentBatch, _cancellationTokenSource.Token);
                }
                catch
                {
                    // ignored
                }

                _currentBatch.Clear();
            }
        }
    }
}
