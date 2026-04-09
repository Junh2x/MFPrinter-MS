using Scanlink.Models;

namespace Scanlink.Core;

/// <summary>
/// 브랜드 공통 복합기 드라이버 인터페이스.
/// 기능 구현 시 브랜드별로 이 인터페이스를 구현한다.
/// </summary>
public interface IMfpDriver
{
    MfpBrand Brand { get; }

    // 연결
    Task<bool> ConnectAsync(string ip, string user = "", string password = "");

    // 스캔함 관리
    Task<List<ScanBox>> GetScanBoxListAsync();
    Task<bool> AddScanBoxAsync(ScanBox box);
    Task<bool> UpdateScanBoxAsync(ScanBox box);
    Task<bool> DeleteScanBoxAsync(string boxId);
}
