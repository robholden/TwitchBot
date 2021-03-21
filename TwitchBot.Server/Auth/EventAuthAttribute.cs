using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TwitchBot.Server.Domain;

namespace TwitchBot.Server.Auth
{
    public sealed class EventAuthAttribute : TypeFilterAttribute
    {
        public EventAuthAttribute() : base(typeof(EventAuthFilter))
        {
        }
    }

    public class EventAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly ILogger<EventAuthAttribute> _logger;

        public EventAuthFilter(ILogger<EventAuthAttribute> logger)
        {
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var req = context.HttpContext.Request;
                if (!req.Headers.TryGetValue("Access-Token", out var token))
                {
                    token = req.Query["access_token"];

                    if (string.IsNullOrEmpty(token))
                    {
                        throw new Exception("Missing access token");
                    }
                }

                using var db = new TwitchBotContext();

                var eventToken = await db.EventTokens.AsNoTracking().FirstOrDefaultAsync(x => x.Token == token);
                if (eventToken == null)
                {
                    throw new Exception("Invalid access token");
                }

                context.HttpContext.Items.Add("UserId", eventToken.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EventAuth Failed");
                context.Result = new ForbidResult();
            }
        }
    }
}