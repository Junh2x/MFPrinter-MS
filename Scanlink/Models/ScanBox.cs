namespace Scanlink.Models;

public class ScanBox
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public string MfpDeviceId { get; set; } = "";

    /// <summary>복합기 주소록 내 슬롯 번호</summary>
    public int SlotIndex { get; set; } = -1;

    // 스캔 파일 설정
    public string FileFormat { get; set; } = "PDF";
    public string ColorMode { get; set; } = "자동컬러";
    public int Resolution { get; set; } = 300;

    // 저장 설정
    public string LocalFolder { get; set; } = "";
    public string DeleteCycle { get; set; } = "사용안함";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
