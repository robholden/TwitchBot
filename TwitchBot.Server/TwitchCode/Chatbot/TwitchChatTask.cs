using System;
using System.Collections.Concurrent;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TwitchBot.Server.Domain;

namespace TwitchBot.Server.TwitchCode.Chatbot
{
    public class TwitchChatTask : BackgroundTask<TwitchChatTask>
    {
        private static readonly ConcurrentDictionary<string, ITwitchChatService> _users = new();
        private static ConcurrentBag<TwitchUser> _userQueue = new();
        private readonly IServiceScopeFactory _scopeFactory;

        public TwitchChatTask(ILogger<TwitchChatTask> logger, IServiceScopeFactory scopeFactory) : base(logger, 15)
        {
            _scopeFactory = scopeFactory;

            using var db = new TwitchBotContext();

            var eventTokens = db.EventTokens.ToList();
            foreach (var et in eventTokens)
            {
                logger.LogInformation($"Connecting to { et.Username } [{ et.Token }]");
            }
            _userQueue = new ConcurrentBag<TwitchUser>(eventTokens.Select(x => new TwitchUser(x.UserId, x.Username)).ToList());
        }

        public static void WatchUser(TwitchUser user)
        {
            if (!_userQueue.Contains(user)) _userQueue.Add(user);
        }

        public static void DisconnectUser(string username)
        {
            if (_users.TryRemove(username, out var service))
            {
                service.Stop();
            }
        }

        public override void DoWork(object state)
        {
            //  Fetch next user in the queue
            if (!_userQueue.TryTake(out var user))
            {
                return;
            }

            // Ensure this user is not connected already
            if (_users.ContainsKey(user.UserId))
            {
                return;
            }

            // Create scope to access injected dependencies
            using var scope = _scopeFactory.CreateScope();

            // Fetch app settings
            var service = scope.ServiceProvider.GetRequiredService<ITwitchChatService>();

            // Connect to Twitch chat
            try
            {
                service.Watch(user);
                _users.TryAdd(user.UserId, service);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start chat bot");
            }
        }

        public new void Dispose()
        {
            foreach (var user in _users.Values)
            {
                user.Stop();
            }

            base.Dispose();
        }
    }
}