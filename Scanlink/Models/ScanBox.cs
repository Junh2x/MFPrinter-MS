namespace Scanlink.Models;

public class ScanBox
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public string MfpDeviceId { get; set; } = "";
    public string MfpEntryId { get; set; } = "";

    // 스캔 파일 설정
    public string FileFormat { get; set; } = "PDF";
    public string ColorMode { get; set; } = "자동컬러";
    public int Resolution { get; set; } = 300;

    // 저장 설정
    public string LocalFolder { get; set; } = "";
    public bool NotifyOnSave { get; set; } = true;
    public string DeleteCycle { get; set; } = "1주";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
