using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

using TwitchBot.Server.Auth;
using TwitchBot.Server.Hubs;
using TwitchBot.Server.Services.Models;

namespace TwitchBot.Server.Controllers
{
    [Controller]
    [Route("sub-callback")]
    public class SubCallbackController : ControllerBase
    {
        private readonly AppSettings _settings;
        private readonly IHubContext<EventHub> _eventHub;

        public SubCallbackController(IOptions<AppSettings> options, IHubContext<EventHub> eventHub)
        {
            _settings = options.Value;
            _eventHub = eventHub;
        }

        [HttpGet]
        public ActionResult<string> Get() => Ok(_settings);

        [TwitchAuth]
        [HttpPost]
        public async Task<ActionResult<string>> Callback([FromBody] CallbackRequest request)
        {
            // Run type specific logic
            switch (HttpContext.GetHeader("Twitch-Eventsub-Message-Type"))
            {
                case "webhook_callback_verification":
                    return Ok(request.Challenge);
            }

            // Is there an event to send?
            if (request.Event != null)
            {
                // Send event to client
                await _eventHub.Clients.User(request.Subscription.Condition.UserId).SendAsync(request.Subscription.Type, request.Event);
            }

            return Ok();
        }

        public record CallbackRequest(string Challenge, Subscription Subscription, object Event);
    }
}