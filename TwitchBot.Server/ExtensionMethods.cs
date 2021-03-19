using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using EnumsNET;

using Microsoft.AspNetCore.Http;

namespace TwitchBot.Server
{
    public static class ExtensionMethods
    {
        public static string GetClaim(this ClaimsPrincipal principal, string type)
        {
            return principal?.Claims?.FirstOrDefault(c => c.Type == type)?.Value;
        }

        public static string GetHeader(this HttpContext context, string key, string returnWhenNull = "")
        {
            if (TryGetHeader(context, key, out var value)) return value;
            return returnWhenNull;
        }

        public static bool TryGetHeader(this HttpContext context, string key, out string value)
        {
            value = string.Empty;
            if (context?.Request?.Headers == null)
            {
                return false;
            }

            if (context.Request.Headers.TryGetValue(key, out var v))
            {
                value = v;
            }

            return !string.IsNullOrEmpty(value);
        }

        public static byte[] HashHMAC(this string message, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.Default.GetBytes(secret));

            // now we can generate the resulting hash
            return hmac.ComputeHash(Encoding.Default.GetBytes(message));
        }

        public static string Description<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
        {
            return enumValue.AsString(EnumFormat.Description);
        }
    }
}