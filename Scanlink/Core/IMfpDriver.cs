using Scanlink.Models;

namespace Scanlink.Core;

/// <summary>
/// 브랜드 공통 복합기 드라이버 인터페이스.
/// 각 브랜드별로 이 인터페이스를 구현한다.
/// </summary>
public interface IMfpDriver
{
    MfpBrand Brand { get; }

    /// <summary>연결 테스트 및 기기 정보 확인</summary>
    Task<DriverResult> ConnectAsync(MfpDevice device);

    /// <summary>최초 연결 시 기기 설정 (캐논: 고급박스 등)</summary>
    Task<DriverResult> SetupAsync(MfpDevice device);

    /// <summary>스캔함(수신지) 목록 조회</summary>
    Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device);

    /// <summary>스캔함 추가 (폴더 생성 + 주소록 등록)</summary>
    Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box);

    /// <summary>스캔함 수정. oldName/oldPassword는 변경 전 값.</summary>
    Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null);

    /// <summary>스캔함 삭제 (주소록 삭제)</summary>
    Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box);

    /// <summary>스캔함 내 파일 목록 조회 (지원 안 하는 브랜드는 빈 리스트 반환)</summary>
    Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box);
}

/// <summary>드라이버 작업 결과</summary>
public class DriverResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<string> Logs { get; set; } = [];

    public static DriverResult Ok(string message = "") => new() { Success = true, Message = message };
    public static DriverResult Fail(string message) => new() { Success = false, Message = message };

    public static DriverResult Fail(string message, List<string> logs) =>
        new() { Success = false, Message = message, Logs = logs };
}

/// <summary>데이터를 포함하는 드라이버 작업 결과</summary>
public class DriverResult<T> : DriverResult
{
    public T? Data { get; set; }

    public static DriverResult<T> Ok(T data, string message = "") =>
        new() { Success = true, Data = data, Message = message };

    public new static DriverResult<T> Fail(string message) =>
        new() { Success = false, Message = message };

    public new static DriverResult<T> Fail(string message, List<string> logs) =>
        new() { Success = false, Message = message, Logs = logs };
}
