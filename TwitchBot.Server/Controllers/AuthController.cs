using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TwitchBot.Server.Domain;
using TwitchBot.Server.Services.Models;
using TwitchBot.Server.TwitchCode.Chatbot;
using TwitchBot.Server.TwitchCode.EventSub;

namespace TwitchBot.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AppSettings _settings;
        private readonly ITwitchEventSubService _service;

        public AuthController(ILogger<AuthController> logger, IOptions<AppSettings> options, ITwitchEventSubService service)
        {
            _logger = logger;
            _settings = options.Value;
            _service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuthResponse>> Auth()
        {
            // Get subscription for user
            var (user, subscriptions) = await _service.Subscribe(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new AuthResponse(subscriptions, new(user.DisplayName, user.UserId, user.ImageUrl)));
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request?.Username))
            {
                return Unauthorized();
            }

            if (!_settings.Logins.TryGetValue(request.Username, out var password) || request.Password != password)
            {
                _logger.LogWarning("Failed login for " + request.Username);
                return Unauthorized();
            }

            // Create subscription for user
            var (user, subscriptions) = await _service.Subscribe(request.Username);
            if (user == null)
            {
                return NotFound();
            }

            // Connect to twitch chat
            TwitchChatTask.WatchUser(user);

            // Create event token for this user
            using var db = new TwitchBotContext();

            var eventToken = await db.EventTokens.FirstOrDefaultAsync(x => x.Username == request.Username.ToLower());
            if (eventToken == null)
            {
                eventToken = new(request.Username.ToLower(), user.UserId);
                await db.AddAsync(eventToken);
                await db.SaveChangesAsync();
            }

            var response = new LoginResponse(eventToken.Token, subscriptions, new(user.DisplayName, user.UserId, user.ImageUrl));
            return Ok(response);
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            var username = User.GetClaim(ClaimTypes.Name);
            await _service.Unsubscribe(User.GetClaim(ClaimTypes.NameIdentifier));
            TwitchChatTask.DisconnectUser(username);

            // Create event token for this user
            using var db = new TwitchBotContext();

            var eventToken = await db.EventTokens.FirstOrDefaultAsync(x => x.Username == username);
            if (eventToken != null)
            {
                db.Remove(eventToken);
                await db.SaveChangesAsync();
            }

            return Ok();
        }
    }

    public record User(string Username, string UserId, string ImageUrl);

    public record AuthResponse(IEnumerable<Subscription> Subscriptions, User User);

    public record LoginResponse(string Token, IEnumerable<Subscription> Subscriptions, User User);

    public record LoginRequest(string Username, string Password);
}