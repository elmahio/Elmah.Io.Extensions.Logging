using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class TStateExtensionsTest
    {
        [Test]
        public void CanGenerateFromFormattedMessage() =>
            Assert.That("Error".Title((s, ex) => $"{ex.GetType().FullName}: {s}", new Exception()), Is.EqualTo("System.Exception: Error"));

        [Test]
        public void CanGenerateFromException()
        {
            var nullReferenceException = new NullReferenceException();
            Assert.That(((string)null).Title(null, new Exception("Error", nullReferenceException)), Is.EqualTo(nullReferenceException.Message));
        }

        [Test]
        public void CanGenerateFromProperties() => Assert.That(new Dictionary<string, object>
        {
            ["AvgDurationMs"] = TimeSpan.FromSeconds(1),
            ["SomeOther"] = 42,
            ["Hello"] = "World"
        }.Title(null, null), Is.EqualTo("[AvgDurationMs, 00:00:01], [SomeOther, 42], [Hello, World]"));

        [Test]
        public void CanShortenProperties() => Assert.That(
            new Dictionary<string, object> { ["1"] = 1, ["2"] = 2, ["3"] = 3, ["4"] = 4, ["5"] = 5, ["6"] = 6 }.Title(null, null),
            Is.EqualTo("[1, 1], [2, 2], [3, 3], [4, 4], [5, 5]"));

        [Test]
        public void CanGenerateGeneric() => Assert.That(((string)null).Title(null, null), Is.EqualTo("Message could not be resolved"));
    }
}
