namespace Toolsfactory.Protocols.Homie.Devices
{
    public record HomieHostConfiguration
    {
        public string TcpServer { get; init; }
        public int Port { get; init; } = 1883;
        public string? Username { get; init; }
        public string? Password { get; init; }
        public bool UseTls { get; init; } = false;
        public ushort ReconnectDelay { get; init; } = 5;
    }
}

