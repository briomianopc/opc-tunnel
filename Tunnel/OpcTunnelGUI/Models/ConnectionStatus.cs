using System;

namespace OpcTunnelGUI.Models;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

public class ConnectionStatus
{
    public ConnectionState State { get; set; } = ConnectionState.Disconnected;
    public string Message { get; set; } = "未连接";
    public DateTime? ConnectedAt { get; set; }
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public int ActiveConnections { get; set; }
}
