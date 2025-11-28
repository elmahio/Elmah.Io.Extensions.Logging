using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Elmah.Io.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Elmah.Io.Extensions.Logging.UserAgentHelper;

namespace Elmah.Io.Extensions.Logging
{
    /// <summary>
    /// An ILoggerProvider for registering the elmah.io logger.
    /// </summary>
    [ProviderAlias("ElmahIo")]
    public class ElmahIoLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ElmahIoProviderOptions options;
        private readonly ICanHandleMessages messageQueue;
        private IExternalScopeProvider scopeProvider;

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
            this.options = options ?? new ElmahIoProviderOptions();

            if (!this.options.Synchronous && this.options.BatchPostingLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ElmahIoLoggerProvider.options.BatchPostingLimit), $"{nameof(ElmahIoLoggerProvider.options.BatchPostingLimit)} must be a positive number.");
            }

            if (!this.options.Synchronous && this.options.Period <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(ElmahIoLoggerProvider.options.Period), $"{nameof(ElmahIoLoggerProvider.options.Period)} must be longer than zero.");
            }

            this.options.ApiKey = apiKey;
            this.options.LogId = logId;

            if (this.options.Synchronous)
                messageQueue = new SynchronousMessageHandler(this.options);
            else
                messageQueue = new MessageQueueHandler(this.options);

            messageQueue.Start();

            CreateInstallation();
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new ElmahIoLogger(categoryName, messageQueue, options, scopeProvider);
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
            messageQueue?.Stop();
        }

        /// <inheritdoc/>
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            this.scopeProvider = scopeProvider;
        }

        private void CreateInstallation()
        {
            try
            {
                var api = ElmahioAPI.Create(options.ApiKey, new ElmahIoOptions
                {
                    WebProxy = options.WebProxy,
                    UserAgent = UserAgent(),
                });

                var logger = new LoggerInfo
                {
                    Type = "Elmah.Io.Extensions.Logging",
                    Properties =
                    [
                        new Item("BackgroundQueueSize", options.BackgroundQueueSize.ToString()),
                        new Item("BatchPostingLimit", options.BatchPostingLimit.ToString()),
                        new Item("IncludeScopes", options.IncludeScopes.ToString()),
                        new Item("Period", options.Period.ToString()),
                        new Item("Synchronous", options.Synchronous.ToString()),
                        new Item("WebProxy", options.WebProxy?.GetType().FullName ?? ""),
                    ],
                    ConfigFiles = [],
                    Assemblies =
                    [
                        new AssemblyInfo { Name = "Elmah.Io.Extensions.Logging", Version = GetType().GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version },
                        new AssemblyInfo { Name = "Elmah.Io.Client", Version = typeof(IElmahioAPI).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version },
                        new AssemblyInfo { Name = "Microsoft.Extensions.Logging", Version = typeof(ILoggerProvider).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version }
                    ],
                    EnvironmentVariables = [],
                };

                var installation = new CreateInstallation
                {
                    Type = ApplicationInfoHelper.GetApplicationType(),
                    Name = options.Application,
                    Loggers = [logger]
                };

                var location = GetType().Assembly.Location;
                var currentDirectory = Path.GetDirectoryName(location);

                var appsettingsFilePath = Path.Combine(currentDirectory, "appsettings.json");
                if (File.Exists(appsettingsFilePath))
                {
                    var appsettingsContent = File.ReadAllText(appsettingsFilePath);

                    using var doc = JsonDocument.Parse(appsettingsContent);
                    using var ms = new MemoryStream();
                    using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true });

                    var root = doc.RootElement;
                    writer.WriteStartObject();

                    var wroteAny = false;

                    if (root.TryGetProperty("Logging", out var loggingSection))
                    {
                        writer.WritePropertyName("Logging");
                        loggingSection.WriteTo(writer);
                        wroteAny = true;
                    }

                    if (root.TryGetProperty("ElmahIo", out var elmahIoSection))
                    {
                        writer.WritePropertyName("ElmahIo");
                        elmahIoSection.WriteTo(writer);
                        wroteAny = true;
                    }

                    writer.WriteEndObject();
                    writer.Flush();

                    if (wroteAny)
                    {
                        var combinedJson = Encoding.UTF8.GetString(ms.ToArray());
                        logger.ConfigFiles.Add(new ConfigFile
                        {
                            Name = Path.GetFileName(appsettingsFilePath),
                            Content = combinedJson,
                            ContentType = "application/json"
                        });
                    }
                }

                // Include environment variables from all possible sources since we don't know in which context Microsoft.Extensions.Logging is being executed.
                EnvironmentVariablesHelper.GetElmahIoAppSettingsEnvironmentVariables().ForEach(v => logger.EnvironmentVariables.Add(v));
                EnvironmentVariablesHelper.GetAspNetCoreEnvironmentVariables().ForEach(v => logger.EnvironmentVariables.Add(v));
                EnvironmentVariablesHelper.GetDotNetEnvironmentVariables().ForEach(v => logger.EnvironmentVariables.Add(v));
                EnvironmentVariablesHelper.GetAzureEnvironmentVariables().ForEach(v => logger.EnvironmentVariables.Add(v));
                EnvironmentVariablesHelper.GetAzureFunctionsEnvironmentVariables().ForEach(v => logger.EnvironmentVariables.Add(v));

                options.OnInstallation?.Invoke(installation);

                api.Installations.CreateAndNotify(options.LogId, installation);
            }
            catch
            {
                // We don't want to crash the entire application if the installation fails. Carry on.
            }
        }
    }
}