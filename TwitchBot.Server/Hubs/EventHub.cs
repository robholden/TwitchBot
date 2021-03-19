using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TwitchBot.Server.Hubs
{
    [Authorize]
    public class EventHub : Hub
    {
    }
}