using RocketPlus.Models;
using System.Collections.Generic;

namespace RocketPlus.Utils
{
    public enum RocketConnectState
    {
        TryConnect = 0,
        Connecting = 1,
        TryDisconnect = 2,
        Disconnect = 3,
    }

    public record FileDataMessage(List<MessageDataModel>? MessageData);
}
