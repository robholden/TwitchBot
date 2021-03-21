using System.Text.Json.Serialization;

namespace TwitchBot.Server.Services.Models
{
    public class Subscription
    {
        public Subscription()
        {
        }

        public Subscription(string type, string userId, AppSettings settings)
        {
            Type = type;
            Condition = new() { UserId = userId };
            Transport = new() { Callback = settings.CallbackUrl, Secret = settings.EventSubSecret };
        }

        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; }

        [JsonPropertyName("status")]
        public string Status { get; init; }

        [JsonPropertyName("version")]
        public string Version { get; init; } = "1";

        [JsonPropertyName("condition")]
        public ConditionRequest Condition { get; init; }

        [JsonPropertyName("transport")]
        public TransportRequest Transport { get; init; }

        public class ConditionRequest
        {
            [JsonPropertyName("broadcaster_user_id")]
            public string UserId { get; init; }
        }

        public class TransportRequest
        {
            [JsonPropertyName("method")]
            public string Method { get; init; } = "webhook";

            [JsonPropertyName("callback")]
            public string Callback { get; init; }

            [JsonPropertyName("secret")]
            public string Secret { get; init; }
        }
    }
}