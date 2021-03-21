using System.Text.Json.Serialization;

namespace TwitchBot.Server.Models
{
    public class FollowerEvent
    {
        [JsonPropertyName("user_name")]
        public string UserName { get; set; }
    }
}