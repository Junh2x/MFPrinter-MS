using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Models;

namespace Scanlink.Drivers.Ricoh;

/// <summary>
/// 리코 복합기 드라이버의 공통 기반.
/// 모델별 드라이버는 이 클래스를 상속하고 플로우만 구현한다.
///
/// 공유 유틸:
///   - HTTP 클라이언트 생성 (리코 표준 헤더)
///   - wimToken 추출 (hidden input)
///   - Base64 인코딩 (리코는 사용자/비밀번호를 base64로 전달)
///   - POST form / GET 헬퍼 — 요청/응답 전체를 HttpExchange로 반환
/// </summary>
public abstract class RicohDriverBase : IMfpDriver
{
    public MfpBrand Brand => MfpBrand.Ricoh;

    protected const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36";

    protected static string B64(string text) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    protected static string? ExtractWimToken(string html)
    {
        var m = Regex.Match(html, @"name=[""']wimToken[""'][^>]*value=[""'](\d+)");
        return m.Success ? m.Groups[1].Value : null;
    }

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
        return (client, cookies);
    }

    /// <summary>
    /// POST form-urlencoded. HttpExchange 전체를 반환해 실패 시 호출자가 Dump() 가능.
    /// </summary>
    protected static async Task<HttpExchange> PostFormAsync(HttpClient client, string url,
        Dictionary<string, string> data, string referer, List<string>? logs = null)
    {
        var bodyText = string.Join("&", data.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value ?? "")}"));

        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(bodyText, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        req.Headers.Add("Referer", referer);
        req.Headers.Add("Upgrade-Insecure-Requests", "1");

        logs?.Add($"[HTTP→] POST ({url})");
        var ex = await HttpDiagnostics.SendAsync(client, req, bodyText);
        logs?.Add($"[HTTP←] {ex.StatusCode} {ex.ReasonPhrase} ({ex.Body.Length}자, {(int)ex.Elapsed.TotalMilliseconds}ms)");
        return ex;
    }

    /// <summary>GET 진단 버전.</summary>
    protected static async Task<HttpExchange> GetAsync(HttpClient client, string url, string? referer = null, List<string>? logs = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(referer)) req.Headers.Add("Referer", referer);
        logs?.Add($"[HTTP→] GET ({url})");
        var ex = await HttpDiagnostics.SendAsync(client, req);
        logs?.Add($"[HTTP←] {ex.StatusCode} {ex.ReasonPhrase} ({ex.Body.Length}자, {(int)ex.Elapsed.TotalMilliseconds}ms)");
        return ex;
    }

    // IMfpDriver — 파생이 구현
    public abstract Task<DriverResult> ConnectAsync(MfpDevice device);
    public abstract Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device);
    public abstract Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box);
    public abstract Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null);
    public abstract Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box);
    public abstract Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box);
    public abstract void DisposeSessions();

    /// <summary>리코 기본: 파일 단위 다운로드 미구현. 필요한 모델은 오버라이드.</summary>
    public virtual Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file)
    {
        return Task.FromResult(DriverResult<byte[]>.Fail("리코 파일 다운로드 미구현"));
    }

    /// <summary>리코는 별도 초기 설정이 필요 없음. 필요한 모델은 오버라이드.</summary>
    public virtual Task<DriverResult> SetupAsync(MfpDevice device)
    {
        device.IsConfigured = true;
        return Task.FromResult(DriverResult.Ok("리코는 별도 초기 설정 불필요"));
    }
}
