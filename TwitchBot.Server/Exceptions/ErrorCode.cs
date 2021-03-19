using System.ComponentModel;

namespace TwitchBot.Server
{
    public enum ErrorCode
    {
        [Description("An unexpected error has occurred")]
        Default,

        [Description("Cannot find {0}")]
        MissingEntity,

        [Description("Request invalid: {0}")]
        InvalidModelRequest,
    }
}