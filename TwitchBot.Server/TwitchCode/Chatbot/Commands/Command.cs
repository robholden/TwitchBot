using TwitchLib.Client.Models;

namespace TwitchBot.Server.TwitchCode.Chatbot.Commands
{
    public record CommandResponse(string chatMessage, object data);

    public interface ICommand
    {
        CommandResponse GetValue(ChatCommand command);
    }
}