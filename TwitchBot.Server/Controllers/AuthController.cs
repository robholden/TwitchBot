using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

            var (userId, subscriptions) = await _service.Subscribe(request.Username);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, userId) };
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.JwtIssuerKey));
            var jwt = new JwtSecurityToken(
                issuer: "TwitchBot",
                audience: "Everyone",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddYears(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new LoginResponse(token, subscriptions);

            return Ok(response);
        }

        [Authorize]
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