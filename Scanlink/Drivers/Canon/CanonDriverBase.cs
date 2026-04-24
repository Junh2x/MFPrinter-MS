using System.Net;
using System.Net.Http;
using System.Text;
using Scanlink.Core;
using Scanlink.Models;

namespace Scanlink.Drivers.Canon;

/// <summary>
/// 캐논 복합기 드라이버의 공통 기반.
/// 모델별 드라이버는 이 클래스를 상속하고 플로우만 구현한다.
///
/// 공유 유틸:
///   - HTTP 클라이언트 생성 (캐논 표준 헤더)
///   - User-Agent 상수
///   - 타임스탬프 헬퍼 (캐논 CGI의 Dummy 파라미터용)
///   - GET/POST 진단 헬퍼 (요청/응답 전체를 HttpExchange로 반환)
///
/// 세션 캐시는 각 파생 드라이버가 자체 관리한다 (플로우별로 세션 형태가 다를 수 있음).
/// </summary>
public abstract class CanonDriverBase : IMfpDriver
{
    public MfpBrand Brand => MfpBrand.Canon;

    protected const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

    /// <summary>캐논 CGI의 Dummy 파라미터용 (캐시 방지).</summary>
    protected static string Dummy() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

    /// <summary>캐논 표준 헤더를 가진 HttpClient 생성.</summary>
    protected static (HttpClient client, CookieContainer cookies) CreateClient()
    {
        var cookies = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            UseCookies = true,
            AllowAutoRedirect = true,
        };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        client.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9");
        client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        return (client, cookies);
    }

    /// <summary>GET 진단 헬퍼.</summary>
    protected static async Task<HttpExchange> GetAsync(HttpClient client, string url, string? referer = null, List<string>? logs = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(referer)) req.Headers.Add("Referer", referer);
        logs?.Add($"[HTTP→] GET ({url})");
        var ex = await HttpDiagnostics.SendAsync(client, req);
        logs?.Add($"[HTTP←] {ex.StatusCode} {ex.ReasonPhrase} ({ex.Body.Length}자, {(int)ex.Elapsed.TotalMilliseconds}ms)");
        return ex;
    }

    /// <summary>POST form-urlencoded 진단 헬퍼. formBody를 그대로 전송.</summary>
    protected static async Task<HttpExchange> PostFormAsync(HttpClient client, string url, string formBody, string referer, List<string>? logs = null, string? origin = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(formBody, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        req.Headers.Add("Referer", referer);
        if (!string.IsNullOrEmpty(origin)) req.Headers.Add("Origin", origin);
        logs?.Add($"[HTTP→] POST ({url})");
        var ex = await HttpDiagnostics.SendAsync(client, req, formBody);
        logs?.Add($"[HTTP←] {ex.StatusCode} {ex.ReasonPhrase} ({ex.Body.Length}자, {(int)ex.Elapsed.TotalMilliseconds}ms)");
        return ex;
    }

    /// <summary>이미지/바이너리 GET 진단 헬퍼.</summary>
    protected static async Task<(HttpExchange ex, byte[] bytes)> GetBytesAsync(HttpClient client, string url, string? referer = null, List<string>? logs = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(referer)) req.Headers.Add("Referer", referer);
        logs?.Add($"[HTTP→] GET ({url})");
        var (ex, bytes) = await HttpDiagnostics.SendBytesAsync(client, req);
        logs?.Add($"[HTTP←] {ex.StatusCode} {ex.ReasonPhrase} ({bytes.Length}바이트, {(int)ex.Elapsed.TotalMilliseconds}ms)");
        return (ex, bytes);
    }

    // IMfpDriver — 파생이 구현
    public abstract Task<DriverResult> ConnectAsync(MfpDevice device);
    public abstract Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device);
    public abstract Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box);
    public abstract Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null);
    public abstract Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box);
    public abstract Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box);
    public abstract void DisposeSessions();

    /// <summary>캐논 기본: 단일 파일 다운로드 미지원(페이지 단위 다운로드 사용). 필요한 모델은 오버라이드.</summary>
    public virtual Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file)
    {
        return Task.FromResult(DriverResult<byte[]>.Fail("캐논은 파일 단위 다운로드를 지원하지 않습니다."));
    }

    /// <summary>캐논 박스는 별도 초기 설정이 필요 없음. 필요한 모델은 오버라이드.</summary>
    public virtual Task<DriverResult> SetupAsync(MfpDevice device)
    {
        device.IsConfigured = true;
        return Task.FromResult(DriverResult.Ok("캐논 박스는 별도 초기 설정 불필요"));
    }
}
