using System.Collections.Generic;

namespace TwitchBot.Server
{
    public class AppSettings
    {
        public string AppUrl { get; set; }

        public IDictionary<string, string> Logins { get; set; }

        public string[] SubscriptionTypes { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string EventSubSecret { get; set; }

        public BotCredentials BotCredentials { get; set; }

        public string CallbackUrl => $"{ AppUrl }/sub-callback";
    }

    public class BotCredentials
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}