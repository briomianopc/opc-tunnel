namespace OpcTunnelGUI.Models;

public class TunnelConfig
{
    public string ListenAddress { get; set; } = "127.0.0.1:30000";
    public string ServerAddress { get; set; } = "";
    public string ServerIP { get; set; } = "";
    public string Token { get; set; } = "";
    public string DnsServer { get; set; } = "dns.alidns.com/dns-query";
    public string EchDomain { get; set; } = "cloudflare-ech.com";
    public bool UseFallback { get; set; } = false;
    public int NumConnections { get; set; } = 1;
    public string TransportMode { get; set; } = "ws";
    public bool EnableTunMode { get; set; } = false;
    public string TunIP { get; set; } = "10.0.85.2";
    public string TunGateway { get; set; } = "10.0.85.1";
    public string TunMask { get; set; } = "255.255.255.0";
    public string TunDNS { get; set; } = "1.1.1.1";
    public bool EnableSystemProxy { get; set; } = false;
}
