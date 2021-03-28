using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TwitchBot.Server.Domain;

namespace TwitchBot.Server.Auth
{
    public static class EventTokenAuthenticationExtensions
    {
        public static AuthenticationBuilder AddEventToken(this AuthenticationBuilder builder, Action<EventTokenAuthorizationOptions> configureOptions = null)
        {
            return builder.AddScheme<EventTokenAuthorizationOptions, EventTokenAuthorizeHandler>(EventTokenAuthenticationDefaults.AuthenticationScheme, configureOptions);
        }
    }

    public static class EventTokenAuthenticationDefaults
    {
        public const string AuthenticationScheme = "EventToken";
    }

    public class EventTokenAuthorizationOptions : AuthenticationSchemeOptions
    {
    }

    public class EventTokenAuthorizeHandler : AuthenticationHandler<EventTokenAuthorizationOptions>
    {
        public EventTokenAuthorizeHandler(IOptionsMonitor<EventTokenAuthorizationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Logger.LogInformation(Request.Path);

            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                if (!TryGetToken(out var token))
                {
                    return AuthenticateResult.NoResult();
                }

                using var db = new TwitchBotContext();

                var eventToken = await db.EventTokens.AsNoTracking().FirstOrDefaultAsync(x => x.Token == token);
                if (eventToken == null)
                {
                    throw new Exception("Invalid access token");
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, eventToken.UserId),
                    new Claim(ClaimTypes.Name, eventToken.Username),
                    new Claim(ClaimTypes.Sid, eventToken.Token)
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        private bool TryGetToken(out string eventToken)
        {
            eventToken = "";

            if (!Request.Headers.TryGetValue("Authorization", out var token) && !Request.Headers.TryGetValue("Access-Token", out token) && !Request.Query.TryGetValue("access_token", out token))
            {
                return false;
            }

            eventToken = token.ToString().Replace("Bearer ", "");
            return true;
        }
    }
}