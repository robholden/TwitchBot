using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TwitchBot.Server.Auth
{
    public sealed class TwitchAuthAttribute : TypeFilterAttribute
    {
        public TwitchAuthAttribute() : base(typeof(TwitchAuthFilter))
        {
        }
    }

    public class TwitchAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly ILogger<TwitchAuthAttribute> _logger;
        private readonly AppSettings _settings;

        public TwitchAuthFilter(ILogger<TwitchAuthAttribute> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var bodyStr = "";
                var req = context.HttpContext.Request;

                // Allows using several time the stream in ASP.Net Core
                req.EnableBuffering();

                // Arguments: Stream, Encoding, detect encoding, buffer size
                // AND, the most important: keep stream opened
                using (var reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                }

                // Rewind, so the core is not lost when it looks the body for the request
                req.Body.Position = 0;

                // Do whatever work with bodyStr here
                if (!req.Headers.TryGetValue("Twitch-Eventsub-Message-Timestamp", out var timestamp) || !DateTime.TryParse(timestamp, out var dt) || dt < DateTime.UtcNow.AddMinutes(-10))
                {
                    throw new Exception("Missing or expired timestamp header");
                }

                if (!req.Headers.TryGetValue("Twitch-Eventsub-Message-Id", out var messageId))
                {
                    throw new Exception("Missing message id header");
                }

                if (!req.Headers.TryGetValue("Twitch-Eventsub-Message-Signature", out var expectedSignature))
                {
                    throw new Exception("Missing message signature header");
                }

                var signatureBytes = (messageId + timestamp + bodyStr).HashHMAC(_settings.EventSubSecret);
                var signature = $"sha256={ BitConverter.ToString(signatureBytes).Replace("-", "").ToLower() }";

                if (expectedSignature != signature)
                {
                    throw new Exception("Signature invalid");
                }

                _logger.LogInformation(bodyStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TwitchAuth Failed");
                context.Result = new ForbidResult();
            }
        }
    }
}