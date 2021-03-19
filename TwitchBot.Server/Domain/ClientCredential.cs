using System;

namespace TwitchBot.Server.Domain
{
    public class ClientCredential
    {
        public int Id { get; set; }

        public string AccessToken { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}