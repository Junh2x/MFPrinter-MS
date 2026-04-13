namespace Scanlink.Models;

/// <summary>
/// 스캔함(박스) 내 파일 정보.
/// </summary>
public class BoxFile
{
    public string DocId { get; set; } = "";
    public string Name { get; set; } = "";
    public int PageCount { get; set; } = 1;
    public string Size { get; set; } = "";    // 예: "A4"
    public DateTime? ScannedAt { get; set; }
}
