using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TwitchBot.Server.Auth;
using TwitchBot.Server.Domain;
using TwitchBot.Server.Services;
using TwitchBot.Server.Services.Models;

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
            var (userId, subscriptions) = await _service.Subscribe(request.Username);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            // Create event token for this user
            using var db = new TwitchBotContext();

            var eventToken = await db.EventTokens.FirstOrDefaultAsync(x => x.Username == request.Username.ToLower());
            if (eventToken == null)
            {
                eventToken = new(request.Username.ToLower(), userId);
                await db.AddAsync(eventToken);
                await db.SaveChangesAsync();
            }

            var response = new LoginResponse(eventToken.Token, subscriptions);
            return Ok(response);
        }

        [EventAuth]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            await _service.Unsubscribe(User.GetClaim(ClaimTypes.NameIdentifier));
            return Ok();
        }
    }

    public record LoginResponse(string Token, IEnumerable<Subscription> Subscriptions);

    public record LoginRequest(string Username, string Password);
}