using System;
using System.IO;
using System.Reflection;
using Elmah.Io.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using static Elmah.Io.Extensions.Logging.UserAgentHelper;

namespace Elmah.Io.Extensions.Logging
{
    /// <summary>
    /// An ILoggerProvider for registering the elmah.io logger.
    /// </summary>
    [ProviderAlias("ElmahIo")]
    public class ElmahIoLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ElmahIoProviderOptions _options;
        private readonly ICanHandleMessages _messageQueue;
        private IExternalScopeProvider _scopeProvider;

        /// <summary>
        /// Create a new instance using the provided options.
        /// </summary>
        /// <param name="options"></param>
        public ElmahIoLoggerProvider(IOptions<ElmahIoProviderOptions> options) : this(options.Value.ApiKey, options.Value.LogId, options.Value)
        {
        }

        /// <summary>
        /// Create a new instance using the provided API key, log ID, and options.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="logId"></param>
        /// <param name="options"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3928:Parameter names used into ArgumentException constructors should match an existing one ", Justification = "Properties are on options")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "Properties are on options")]
        public ElmahIoLoggerProvider(string apiKey, Guid logId, ElmahIoProviderOptions options = null)
        {
            _options = options ?? new ElmahIoProviderOptions();

            if (!_options.Synchronous && _options.BatchPostingLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.BatchPostingLimit), $"{nameof(_options.BatchPostingLimit)} must be a positive number.");
            }

            if (!_options.Synchronous && _options.Period <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.Period), $"{nameof(_options.Period)} must be longer than zero.");
            }

            _options.ApiKey = apiKey;
            _options.LogId = logId;

            if (_options.Synchronous)
                _messageQueue = new SynchronousMessageHandler(_options);
            else
                _messageQueue = new MessageQueueHandler(_options);

            _messageQueue.Start();

            CreateInstallation();
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new ElmahIoLogger(categoryName, _messageQueue, _options, _scopeProvider);
        }

        /// <summary>
        /// Dispose the internal message queue, trying to process all pending messages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method can be overriden by a subclass to do custom cleanup. Make sure to call: base.Dispose(disposing)
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            _messageQueue?.Stop();
        }

        /// <inheritdoc/>
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        private void CreateInstallation()
        {
            try
            {
                var api = ElmahioAPI.Create(_options.ApiKey, new ElmahIoOptions
                {
                    WebProxy = _options.WebProxy,
                    UserAgent = UserAgent(),
                });

                var logger = new LoggerInfo
                {
                    Type = "Elmah.Io.Extensions.Logging",
                    Properties =
                    [
                        new Item("BackgroundQueueSize", _options.BackgroundQueueSize.ToString()),
                        new Item("BatchPostingLimit", _options.BatchPostingLimit.ToString()),
                        new Item("IncludeScopes", _options.IncludeScopes.ToString()),
                        new Item("Period", _options.Period.ToString()),
                        new Item("Synchronous", _options.Synchronous.ToString()),
                        new Item("WebProxy", _options.WebProxy?.GetType().FullName ?? ""),
                    ],
                    ConfigFiles = [],
                    Assemblies =
                    [
                        new AssemblyInfo { Name = "Elmah.Io.Extensions.Logging", Version = GetType().GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version },
                        new AssemblyInfo { Name = "Elmah.Io.Client", Version = typeof(IElmahioAPI).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version },
                        new AssemblyInfo { Name = "Microsoft.Extensions.Logging", Version = typeof(ILoggerProvider).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version }
                    ],
                };

                var installation = new CreateInstallation
                {
                    Type = ApplicationInfoHelper.GetApplicationType(),
                    Name = _options.Application,
                    Loggers = [logger]
                };

                var location = GetType().Assembly.Location;
                var currentDirectory = Path.GetDirectoryName(location);

                var appsettingsFilePath = Path.Combine(currentDirectory, "appsettings.json");
                if (File.Exists(appsettingsFilePath))
                {
                    var combinedSections = new JObject();

                    var appsettingsContent = File.ReadAllText(appsettingsFilePath);
                    var appsettingsObject = JObject.Parse(appsettingsContent);
                    if (appsettingsObject.TryGetValue("Logging", out JToken loggingSection))
                    {
                        combinedSections.Add("Logging", loggingSection.DeepClone());
                    }

                    if (appsettingsObject.TryGetValue("ElmahIo", out JToken elmahIoSection))
                    {
                        combinedSections.Add("ElmahIo", elmahIoSection.DeepClone());
                    }

                    if (combinedSections.HasValues)
                    {
                        logger.ConfigFiles.Add(new ConfigFile
                        {
                            Name = Path.GetFileName(appsettingsFilePath),
                            Content = combinedSections.ToString(),
                            ContentType = "application/json"
                        });
                    }
                }

                api.Installations.Create(_options.LogId.ToString(), installation);
            }
            catch
            {
                // We don't want to crash the entire application if the installation fails. Carry on.
            }
        }
    }
}