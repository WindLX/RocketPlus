namespace RocketPlus.Utils
{
    public enum RocketConnectState
    {
        TryConnect = 0,
        Connecting = 1,
        TryDisconnect = 2,
        Disconnect = 3,
    }

    public record FilePathMessage(string filePath);
}
