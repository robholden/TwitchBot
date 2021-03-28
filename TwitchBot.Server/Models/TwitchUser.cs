using System.Text.Json.Serialization;

namespace TwitchBot.Server.Domain
{
    public class TwitchUser
    {
        public TwitchUser()
        {
        }

        public TwitchUser(string userId, string username)
        {
            UserId = userId;
            DisplayName = username;
        }

        [JsonPropertyName("id")]
        public string UserId { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("profile_image_url")]
        public string ImageUrl { get; set; }
    }
}