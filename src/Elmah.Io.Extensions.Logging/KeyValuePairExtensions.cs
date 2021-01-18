using Elmah.Io.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elmah.Io.Extensions.Logging
{
    internal static class KeyValuePairExtensions
    {
        internal static bool Is(this KeyValuePair<string, object> keyValue, string field)
        {
            if (string.IsNullOrWhiteSpace(keyValue.Key)) return false;
            return string.Equals(keyValue.Key, field, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool Is(this KeyValuePair<string, object> keyValue, string field, out string result)
        {
            result = null;
            if (!keyValue.Is(field)) return false;
            result = keyValue.Value?.ToString();
            return true;
        }

        internal static bool IsStatusCode(this KeyValuePair<string, object> keyValue, out int? statusCode)
        {
            statusCode = null;
            if (!keyValue.Is("statuscode")) return false;
            if (keyValue.Value == null || string.IsNullOrWhiteSpace(keyValue.Value.ToString())) return false;
            if (!int.TryParse(keyValue.Value.ToString(), out int code)) return false;
            statusCode = code;
            return true;
        }

        internal static bool IsApplication(this KeyValuePair<string, object> keyValue, out string application)
        {
            return keyValue.Is("application", out application);
        }

        internal static bool IsSource(this KeyValuePair<string, object> keyValue, out string source)
        {
            return keyValue.Is("source", out source);
        }

        internal static bool IsHostname(this KeyValuePair<string, object> keyValue, out string hostname)
        {
            return keyValue.Is("hostname", out hostname);
        }

        internal static bool IsUser(this KeyValuePair<string, object> keyValue, out string user)
        {
            return keyValue.Is("user", out user);
        }

        internal static bool IsMethod(this KeyValuePair<string, object> keyValue, out string method)
        {
            return keyValue.Is("method", out method);
        }

        internal static bool IsVersion(this KeyValuePair<string, object> keyValue, out string version)
        {
            return keyValue.Is("version", out version);
        }

        internal static bool IsUrl(this KeyValuePair<string, object> keyValue, out string url)
        {
            return keyValue.Is("url", out url);
        }

        internal static bool IsType(this KeyValuePair<string, object> keyValue, out string type)
        {
            return keyValue.Is("type", out type);
        }

        internal static bool IsCorrelationId(this KeyValuePair<string, object> keyValue, out string correlationId)
        {
            return keyValue.Is("correlationid", out correlationId);
        }

        internal static bool IsServerVariables(this KeyValuePair<string, object> keyValue, out List<Item> serverVariables)
        {
            serverVariables = null;
            if (!keyValue.Is("servervariables")) return false;
            serverVariables = keyValue.ToItemList();
            return true;
        }

        internal static bool IsCookies(this KeyValuePair<string, object> keyValue, out List<Item> cookies)
        {
            cookies = null;
            if (!keyValue.Is("cookies")) return false;
            cookies = keyValue.ToItemList();
            return true;
        }

        internal static bool IsForm(this KeyValuePair<string, object> keyValue, out List<Item> form)
        {
            form = null;
            if (!keyValue.Is("form")) return false;
            form = keyValue.ToItemList();
            return true;
        }

        internal static bool IsQueryString(this KeyValuePair<string, object> keyValue, out List<Item> queryString)
        {
            queryString = null;
            if (!keyValue.Is("querystring")) return false;
            queryString = keyValue.ToItemList();
            return true;
        }

        internal static List<Item> ToItemList(this KeyValuePair<string, object> property)
        {
            List<Item> result = new List<Item>();

            if (property.Value is IEnumerable<KeyValuePair<string, object>> properties)
            {
                result = properties.Select(p => ToItem(p)).ToList();
            }
            else if (property.Value is Dictionary<string, string> dictionary)
            {
                foreach (var key in dictionary.Keys)
                {
                    var value = dictionary[key];
                    result.Add(new Item(key, value != null ? value.Trim() : value));
                }
            }
            else if (property.Value is string && !string.IsNullOrWhiteSpace(property.Value?.ToString()))
            {
                var keyValues = property.Value.ToString().Split(new string[] { "], [" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var rde in keyValues)
                {
                    var keyAndValue = rde.TrimStart('[').TrimEnd(']').Split(',');
                    if (keyAndValue.Length != 2) continue;
                    var key = keyAndValue[0];
                    var value = keyAndValue[1];
                    if (string.IsNullOrWhiteSpace(key)) continue;
                    key = key.Trim();
                    value = value != null ? value.Trim() : value;
                    result.Add(new Item(key, value));
                }
            }

            return result;
        }

        internal static Item ToItem(this KeyValuePair<string, object> property)
        {
            return new Item() { Key = property.Key, Value = property.Value?.ToString() };
        }

    }
}
