using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class ElmahIoLoggerProviderTest
    {
        [Test]
        public void CanCreateInstanceWithOptions()
        {
            using var provider = new ElmahIoLoggerProvider(Options.Create(new ElmahIoProviderOptions()));
            var logger = provider.CreateLogger("test");
            Assert.NotNull(provider);
            Assert.NotNull(logger);
        }

        [Test]
        public void CanCreateInstanceWithOnlyRequiredParameters()
        {
            using var provider = new ElmahIoLoggerProvider("apiKey", Guid.NewGuid());
            var logger = provider.CreateLogger("test");
            Assert.NotNull(provider);
            Assert.NotNull(logger);
        }
    }
}
