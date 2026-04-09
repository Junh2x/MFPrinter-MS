namespace Scanlink.Models;

public enum MfpBrand
{
    Unknown,
    Canon,
    Ricoh,
    Sindoh,
}

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error,
}

public class MfpDevice
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Ip { get; set; } = "";
    public MfpBrand Brand { get; set; } = MfpBrand.Unknown;
    public string Model { get; set; } = "";
    public string DisplayName => $"{Brand} {Model}".Trim();
    public int Port { get; set; } = 80;
    public string BaseUrl { get; set; } = "";
    public ConnectionStatus Status { get; set; } = ConnectionStatus.Disconnected;
    public DateTime AddedAt { get; set; } = DateTime.Now;
}
