using System;
using System.Collections.Generic;

using TwitchLib.Client.Models;

namespace TwitchBot.Server.TwitchCode.Chatbot.Commands
{
    public class DroppingCommand : ICommand
    {
        public CommandResponse GetValue(ChatCommand command)
        {
            var pairs = new List<string>() { "B8" };
            var letters = "CDEFGH";

            for (var i = 2; i <= 7; i++)
            {
                for (var j = 0; j < letters.Length; j++) pairs.Add(letters[j] + i.ToString());
            }

            var marker = pairs[RandomNum(0, pairs.Count - 1)];
            return new CommandResponse($"@{ command.ChatMessage.Username } has requested you to drop at { marker }", marker);
        }

        private static int RandomNum(int min, int max)
        {
            if (min > max)
            {
                var n = min;
                min = max;
                max = n;
            }

            var r = new Random();
            return r.Next(min, max);
        }
    }
}