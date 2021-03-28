using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TwitchBot.Server.Domain;
using TwitchBot.Server.Services.Models;

namespace TwitchBot.Server.TwitchCode.EventSub
{
    public interface ITwitchEventSubService
    {
        Task<(TwitchUser user, IList<Subscription> subscriptions)> Subscribe(string username);

        Task<bool> Unsubscribe(Subscription sub);

        Task<bool> Unsubscribe(string userId);
    }

    public class TwitchEventSubService : ITwitchEventSubService
    {
        private readonly ILogger<ITwitchEventSubService> _logger;
        private readonly AppSettings _settings;
        private readonly HttpClient _client;

        public TwitchEventSubService(ILogger<ITwitchEventSubService> logger, IOptions<AppSettings> options, HttpClient client)
        {
            _logger = logger;
            _settings = options.Value;
            _client = client;
        }

        public async Task<(TwitchUser user, IList<Subscription> subscriptions)> Subscribe(string username)
        {
            // Get user id
            var user = await GetUser(username);
            if (user == null) return (null, default);

            // Find which subs we are already subscribed to
            var subs = new List<Subscription>();
            var currentTypes = await CurrentSubscriptions(user.UserId);
            if (currentTypes?.Any() == true)
            {
                subs.AddRange(currentTypes);
            }

            // Create missing subscriptions to Twitch
            foreach (var type in _settings.SubscriptionTypes.Where(type => currentTypes?.Any(ct => ct.Type == type.Key) != true))
            {
                try
                {
                    var sub = await Register(type.Key, user.UserId);
                    if (sub != null) subs.Add(sub);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to register sub { type } for user { user }");
                }
            }

            // Assign methods to subs
            foreach (var sub in subs)
            {
                if (!_settings.SubscriptionTypes.TryGetValue(sub.Type, out var method)) continue;

                sub.Method = method;
            }

            return (user, subs);
        }

        public async Task<bool> Unsubscribe(string userId)
        {
            try
            {
                // Loop through each sub and delete
                foreach (var sub in await CurrentSubscriptions(userId))
                {
                    await Unsubscribe(sub);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to deregister subs for user { userId }");
                return false;
            }
        }

        public async Task<bool> Unsubscribe(Subscription sub)
        {
            // Add authorization headers
            if (!await AddOAuth()) return default;

            try
            {
                await _client.DeleteAsync("https://api.twitch.tv/helix/eventsub/subscriptions?id=" + sub.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to deregister subs for sub id { sub.Id}");
                return false;
            }
        }

        private async Task<TwitchUser> GetUser(string username)
        {
            // Add authorization headers
            if (!await AddOAuth()) return default;

            try
            {
                var response = await _client.GetFromJsonAsync<TwitchResponse<TwitchUser>>("https://api.twitch.tv/helix/users?login=" + username);
                return response?.Data?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get user info for { username }");
                return default;
            }
        }

        private async Task<IEnumerable<Subscription>> CurrentSubscriptions(string userId)
        {
            // Add authorization headers
            if (!await AddOAuth()) return default;

            try
            {
                var subsResponse = await _client.GetFromJsonAsync<TwitchResponse<Subscription>>("https://api.twitch.tv/helix/eventsub/subscriptions");
                var subs = subsResponse?.Data?.Where(x => x.Condition?.UserId == userId)?.ToList();

                // Delete subs with incorrect callback
                foreach (var sub in subs.Where(x => x.Transport.Callback != _settings.CallbackUrl || x.Status == "webhook_callback_verification_failed").ToList())
                {
                    await Unsubscribe(sub);
                    subs.Remove(sub);
                }

                return subs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get subscriptions for user id { userId }");
                return default;
            }
        }

        private async Task<Subscription> Register(string type, string userId)
        {
            // Add authorization headers
            if (!await AddOAuth()) return default;

            try
            {
                var subscription = new Subscription(type, userId, _settings);
                var response = await _client.PostAsJsonAsync("https://api.twitch.tv/helix/eventsub/subscriptions", subscription);
                var message = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(message);
                    response.EnsureSuccessStatusCode();
                }

                var result = JsonSerializer.Deserialize<TwitchResponse<Subscription>>(message);
                return result?.Data?.FirstOrDefault();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Api call failed to register { type } for user { userId }");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal error registering type { type } for user { userId }");
                return null;
            }
        }

        private async Task<bool> AddOAuth()
        {
            // Check for existing credentials
            using var db = new TwitchBotContext();

            var creds = await db.ClientCredentials.FirstOrDefaultAsync(x => x.ExpiryDate > DateTime.UtcNow);
            if (creds == null)
            {
                // Get app access token from Twitch
                var oauth = await GetOAuth();
                if (oauth == null)
                {
                    return false;
                }

                // Add entry to the db
                creds = new()
                {
                    AccessToken = oauth.AccessToken,
                    ExpiryDate = DateTime.UtcNow.AddSeconds(oauth.ExpiresIn)
                };
                creds = (await db.AddAsync(creds)).Entity;
                await db.SaveChangesAsync();
            }

            // Add authorizaton headers for Twitch Apis
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Client-ID", _settings.ClientId);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer { creds.AccessToken }");

            return true;
        }

        private async Task<OAuthResponse> GetOAuth()
        {
            try
            {
                // https://dev.twitch.tv/docs/authentication
                var response = await _client.PostAsJsonAsync($"https://id.twitch.tv/oauth2/token?client_id={ _settings.ClientId }&client_secret={ _settings.ClientSecret }&grant_type=client_credentials", "");
                var message = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(message);
                    response.EnsureSuccessStatusCode();
                }

                return JsonSerializer.Deserialize<OAuthResponse>(message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Api call failed to generate access token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error generating access token");
                return null;
            }
        }

        private class OAuthResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }

        private class TwitchResponse<T>
        {
            [JsonPropertyName("data")]
            public T[] Data { get; set; }
        }

        private class IdResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
        }
    }
}