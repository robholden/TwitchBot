using System;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TwitchBot.Server.Domain;
using TwitchBot.Server.Hubs;
using TwitchBot.Server.TwitchCode.Chatbot.Commands;

using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchBot.Server.TwitchCode.Chatbot
{
    public interface ITwitchChatService
    {
        void Watch(TwitchUser user);

        void Stop();
    }

    public class TwitchChatService : ITwitchChatService
    {
        private readonly ILogger<TwitchChatService> _logger;
        private readonly AppSettings _settings;
        private readonly IHubContext<EventHub> _hub;

        private TwitchUser _user;
        private TwitchClient _twitchClient;

        public TwitchChatService(ILogger<TwitchChatService> logger, IOptions<AppSettings> options, IHubContext<EventHub> hub)
        {
            _logger = logger;
            _settings = options.Value;
            _hub = hub;
        }

        public void Stop()
        {
            Say("Goodbye :(");
            _twitchClient?.Disconnect();
        }

        public void Watch(TwitchUser user)
        {
            var credentials = new ConnectionCredentials(_settings.BotCredentials.Username, _settings.BotCredentials.Password);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            var customClient = new WebSocketClient(clientOptions);

            _twitchClient = new TwitchClient(customClient);
            _twitchClient.Initialize(credentials, user.DisplayName);

            _twitchClient.OnJoinedChannel += Client_OnJoinedChannel;
            _twitchClient.OnChatCommandReceived += Client_OnChatCommandReceived;

            _twitchClient.Connect();
            _user = user;
        }

        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            // Match message with command
            var command = e.Command.CommandText switch
            {
                "dropping" => new DroppingCommand(),
                _ => null
            };

            // Send event to client
            if (command != null)
            {
                var response = command.GetValue(e.Command);
                Say(response.chatMessage);
                _hub.Clients.User(_user.UserId).SendAsync($"command.{ e.Command.CommandText }", response.data);
            }
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Say("Hello :)", true);
        }

        private void Say(string message, bool dryRun = false)
        {
            _logger.LogInformation($"{ _user.DisplayName } => { message }");
            _twitchClient.SendMessage(_user.DisplayName, message, dryRun);
        }
    }
}