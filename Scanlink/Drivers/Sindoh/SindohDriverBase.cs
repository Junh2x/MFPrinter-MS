using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Scanlink.Core;
using Scanlink.Models;

namespace Scanlink.Drivers.Sindoh;

/// <summary>
/// 신도리코 복합기 드라이버의 공통 기반.
/// 모델별 드라이버는 이 클래스를 상속하고 플로우만 구현한다.
///
/// 공유 유틸:
///   - HTTP 클라이언트 생성 (신도 표준 헤더, XMLHttpRequest)
///   - JSON POST 헬퍼 (요청/응답 전체를 캡처한 HttpExchange 반환)
///   - Form POST / 일반 SendAsync 헬퍼 (역시 HttpExchange 반환)
///   - usr 쿠키 경로별 교체 로직
/// </summary>
public abstract class SindohDriverBase : IMfpDriver
{
    public MfpBrand Brand => MfpBrand.Sindoh;

    protected const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36";

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
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        return (client, cookies);
    }

    /// <summary>
    /// POST JSON. 성공 시 [HTTP→]/[HTTP←] 요약 로그를 남기고, HttpExchange 전체를 반환.
    /// 호출자는 실패 판정 후 result.Logs.Add(ex.Dump())로 전체 덤프를 쓸 수 있다.
    /// </summary>
    protected static async Task<HttpExchange> PostJsonAsync(HttpClient client, string url, object data, string referer, List<string>? logs = null)
    {
        var json = JsonSerializer.Serialize(data);
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        req.Headers.Add("Referer", referer);
        req.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");

        logs?.Add($"[HTTP→] POST ({url})");
        var ex = await HttpDiagnostics.SendAsync(client, req, json);
        logs?.Add($"[HTTP←] {ex.StatusCode} {ex.ReasonPhrase} ({ex.Body.Length}자, {(int)ex.Elapsed.TotalMilliseconds}ms)");
        return ex;
    }

    /// <summary>
    /// POST form-urlencoded. 헤더/본문을 호출자가 직접 구성할 때 사용. HttpExchange 반환.
    /// </summary>
    protected static async Task<HttpExchange> PostFormAsync(HttpClient client, string url, string formBody, string referer, List<string>? logs = null, string accept = "application/json, text/javascript, */*; q=0.01")
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(formBody, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        req.Headers.Add("Referer", referer);
        req.Headers.Add("Accept", accept);

        logs?.Add($"[HTTP→] POST ({url})");
        var ex = await HttpDiagnostics.SendAsync(client, req, formBody);
        logs?.Add($"[HTTP←] {ex.StatusCode} {ex.ReasonPhrase} ({ex.Body.Length}자, {(int)ex.Elapsed.TotalMilliseconds}ms)");
        return ex;
    }

    /// <summary>GET 요청 진단 버전.</summary>
    protected static async Task<HttpExchange> GetAsync(HttpClient client, string url, List<string>? logs = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        logs?.Add($"[HTTP→] GET ({url})");
        var ex = await HttpDiagnostics.SendAsync(client, req);
        logs?.Add($"[HTTP←] {ex.StatusCode} {ex.ReasonPhrase} ({ex.Body.Length}자, {(int)ex.Elapsed.TotalMilliseconds}ms)");
        return ex;
    }

    /// <summary>
    /// usr 쿠키를 주어진 값으로 교체. 기존 usr 쿠키(path=/, path=/wcd 모두) 제거 후 새로 추가.
    /// 서버가 path=/wcd로 설정하기 때문에 path 중복을 피해야 함.
    /// </summary>
    protected static void ReplaceUsrCookie(CookieContainer cookies, Uri baseUri, Uri wcdUri, string value)
    {
        foreach (Cookie c in cookies.GetCookies(wcdUri))
            if (c.Name == "usr") c.Expired = true;
        foreach (Cookie c in cookies.GetCookies(baseUri))
            if (c.Name == "usr") c.Expired = true;

        cookies.Add(wcdUri, new Cookie("usr", value, "/wcd"));
    }

    // IMfpDriver — 파생이 구현
    public abstract Task<DriverResult> ConnectAsync(MfpDevice device);
    public abstract Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device);
    public abstract Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box);
    public abstract Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null);
    public abstract Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box);
    public abstract Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box);
    public abstract Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file);
    public abstract void DisposeSessions();

    /// <summary>신도는 별도 초기 설정이 필요 없음. 필요한 모델은 오버라이드.</summary>
    public virtual Task<DriverResult> SetupAsync(MfpDevice device)
    {
        device.IsConfigured = true;
        return Task.FromResult(DriverResult.Ok("신도는 별도 초기 설정 불필요"));
    }
}
