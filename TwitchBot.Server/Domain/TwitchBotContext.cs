using Microsoft.EntityFrameworkCore;

namespace TwitchBot.Server.Domain
{
    public class TwitchBotContext : DbContext
    {
        public DbSet<ClientCredential> ClientCredentials { get; set; }

        public DbSet<EventToken> EventTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=twitch-bot.db");
    }
}