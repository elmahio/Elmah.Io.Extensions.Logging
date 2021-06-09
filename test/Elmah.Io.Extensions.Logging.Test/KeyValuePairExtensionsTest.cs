using Elmah.Io.Client;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class KeyValuePairExtensionsTest
    {
        [TestCase(500, true, "statusCode", 500)]
        [TestCase(500, true, "statusCode", 500)]
        [TestCase(500, true, "StatusCode", 500)]
        [TestCase(null, false, "StatusCode", "")]
        [TestCase(null, false, "StatusCode", null)]
        [TestCase(null, false, "StatusCode", "GET")]
        [TestCase(null, false, "Other", 500)]
        public void CanCheckIsStatusCode(int? expectedStatusCode, bool expectedIsStatusCode, string key, object value)
        {
            var isStatusCode = new KeyValuePair<string, object>(key, value).IsStatusCode(out int? result);
            Assert.That(isStatusCode, Is.EqualTo(expectedIsStatusCode));
            Assert.That(result, Is.EqualTo(expectedStatusCode));
        }

        [TestCase("MyApp", true, "application", "MyApp")]
        [TestCase("MyApp", true, "Application", "MyApp")]
        [TestCase(null, false, "Other", "MyApp")]
        [TestCase("42", true, "Application", 42)]
        public void CanCheckIs(string expectedValue, bool expectedIsMatch, string key, object value)
        {
            var isMatch = new KeyValuePair<string, object>(key, value).Is("application", out string result);
            Assert.That(isMatch, Is.EqualTo(expectedIsMatch));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void CanCheckSingleServerVariables()
        {
            var isMatch = new KeyValuePair<string, object>("serverVariables", "[Hello, World]").IsServerVariables(out List<Item> result);
            Assert.That(isMatch, Is.True);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Key, Is.EqualTo("Hello"));
            Assert.That(result.First().Value, Is.EqualTo("World"));
        }

        [Test]
        public void CanCheckMultipleServerVariables()
        {
            var isMatch = new KeyValuePair<string, object>("serverVariables", "[Hello, World], [Foo, Bar]").IsServerVariables(out List<Item> result);
            Assert.That(isMatch, Is.True);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.First().Key, Is.EqualTo("Hello"));
            Assert.That(result.First().Value, Is.EqualTo("World"));       
            Assert.That(result.Last().Key, Is.EqualTo("Foo"));
            Assert.That(result.Last().Value, Is.EqualTo("Bar"));
        }
    }
}
