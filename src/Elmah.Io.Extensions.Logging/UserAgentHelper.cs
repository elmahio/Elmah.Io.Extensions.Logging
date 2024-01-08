using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace Elmah.Io.Extensions.Logging
{
    internal static class UserAgentHelper
    {
        private static string _assemblyVersion = typeof(ElmahIoLogger).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        private static string _melAssemblyVersion = typeof(Microsoft.Extensions.Logging.ILogger).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        internal static string UserAgent()
        {
            return new StringBuilder()
                .Append(new ProductInfoHeaderValue(new ProductHeaderValue("Elmah.Io.Extensions.Logging", _assemblyVersion)).ToString())
                .Append(" ")
                .Append(new ProductInfoHeaderValue(new ProductHeaderValue("Microsoft.Extensions.Logging", _melAssemblyVersion)).ToString())
                .ToString();
        }
    }
}
