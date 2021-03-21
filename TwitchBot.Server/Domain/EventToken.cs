using System;

namespace TwitchBot.Server.Domain
{
    public class EventToken
    {
        public EventToken()
        {
        }

        public EventToken(string username, string userId)
        {
            Username = username;
            UserId = userId;
        }

        public int Id { get; set; }

        public string Username { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; } = Guid.NewGuid().ToString();
    }
}