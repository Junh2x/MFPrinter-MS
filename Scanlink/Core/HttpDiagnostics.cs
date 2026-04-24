using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Scanlink.Core;

/// <summary>
/// HTTP 교환(Exchange) 스냅샷. 요청 URL/헤더/본문과 응답 상태/헤더/본문을 모두 보존한다.
/// 실패 원인 분석을 위해 드라이버의 실패 분기에서 Dump()를 호출해 로그에 남긴다.
/// </summary>
public sealed class HttpExchange
{
    public required string Method { get; init; }
    public required string Url { get; init; }
    public required string RequestHeaders { get; init; }
    public required string RequestBody { get; init; }
    public required int StatusCode { get; init; }
    public required string ReasonPhrase { get; init; }
    public required string ResponseHeaders { get; init; }
    public required string Body { get; init; }
    public required TimeSpan Elapsed { get; init; }

    public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode < 300;

    /// <summary>
    /// 다중 라인 디버그 덤프. maxBody로 본문 전/후반을 잘라 과도한 로그 증가를 방지.
    /// </summary>
    public string Dump(int maxBody = 2000)
    {
        var sb = new StringBuilder();
        sb.AppendLine("╔══ HTTP 디버그 ════════════════════════════════════════════");
        sb.AppendLine($"║ {Method} {Url}  ({(int)Elapsed.TotalMilliseconds}ms)");
        sb.AppendLine("║ ── 요청 헤더 ──────────────────────────────────────────────");
        sb.AppendLine(Indent(string.IsNullOrEmpty(RequestHeaders) ? "(없음)" : RequestHeaders));
        sb.AppendLine("║ ── 요청 본문 ──────────────────────────────────────────────");
        sb.AppendLine(Indent(Truncate(string.IsNullOrEmpty(RequestBody) ? "(없음)" : RequestBody, maxBody)));
        sb.AppendLine($"║ ── 응답 {StatusCode} {ReasonPhrase} ────────────────────────────────");
        sb.AppendLine("║ ── 응답 헤더 ──────────────────────────────────────────────");
        sb.AppendLine(Indent(string.IsNullOrEmpty(ResponseHeaders) ? "(없음)" : ResponseHeaders));
        sb.AppendLine("║ ── 응답 본문 ──────────────────────────────────────────────");
        sb.AppendLine(Indent(Truncate(Body, maxBody)));
        sb.Append("╚═══════════════════════════════════════════════════════════");
        return sb.ToString();
    }

    private static string Indent(string s)
    {
        if (string.IsNullOrEmpty(s)) return "║   ";
        var lines = s.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder();
        for (var i = 0; i < lines.Length; i++)
        {
            sb.Append("║   ").Append(lines[i]);
            if (i < lines.Length - 1) sb.Append('\n');
        }
        return sb.ToString();
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
        var head = max / 2;
        var tail = max - head;
        return $"{s[..head]}\n… <{s.Length - max}자 생략> …\n{s[^tail..]}";
    }
}

/// <summary>
/// HTTP 전송 + 진단 스냅샷 수집 헬퍼.
/// 각 드라이버의 공용 base에서 이 헬퍼를 통해 요청을 보내면 HttpExchange가 자동으로 채워진다.
/// </summary>
public static class HttpDiagnostics
{
    /// <summary>
    /// 전달된 HttpRequestMessage를 전송하고 요청/응답 전체 스냅샷을 반환한다.
    /// requestBody는 문자열 기반 Content(JSON, form urlencoded 등)인 경우 호출자가 직접 전달.
    /// (HttpContent는 일부 구현이 스트림 1회 소비만 허용하므로 사전에 읽어 둔 원본 문자열을 넘겨받는다.)
    /// </summary>
    public static async Task<HttpExchange> SendAsync(HttpClient client, HttpRequestMessage req, string? requestBody = null)
    {
        var method = req.Method.Method;
        var url = req.RequestUri?.ToString() ?? "";
        var reqHeaders = FormatHeaders(req.Headers, req.Content?.Headers);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HttpExchange
            {
                Method = method,
                Url = url,
                RequestHeaders = reqHeaders,
                RequestBody = requestBody ?? "",
                StatusCode = 0,
                ReasonPhrase = $"(예외) {ex.GetType().Name}: {ex.Message}",
                ResponseHeaders = "",
                Body = "",
                Elapsed = sw.Elapsed,
            };
        }

        var body = await resp.Content.ReadAsStringAsync();
        sw.Stop();

        return new HttpExchange
        {
            Method = method,
            Url = url,
            RequestHeaders = reqHeaders,
            RequestBody = requestBody ?? "",
            StatusCode = (int)resp.StatusCode,
            ReasonPhrase = resp.ReasonPhrase ?? "",
            ResponseHeaders = FormatHeaders(resp.Headers, resp.Content.Headers),
            Body = body,
            Elapsed = sw.Elapsed,
        };
    }

    /// <summary>바이트 응답용 전송. 이미지/PDF 등 바이너리 엔드포인트에서 사용.</summary>
    public static async Task<(HttpExchange ex, byte[] bytes)> SendBytesAsync(HttpClient client, HttpRequestMessage req, string? requestBody = null)
    {
        var method = req.Method.Method;
        var url = req.RequestUri?.ToString() ?? "";
        var reqHeaders = FormatHeaders(req.Headers, req.Content?.Headers);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return (new HttpExchange
            {
                Method = method,
                Url = url,
                RequestHeaders = reqHeaders,
                RequestBody = requestBody ?? "",
                StatusCode = 0,
                ReasonPhrase = $"(예외) {ex.GetType().Name}: {ex.Message}",
                ResponseHeaders = "",
                Body = "",
                Elapsed = sw.Elapsed,
            }, Array.Empty<byte>());
        }

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        sw.Stop();

        var ct = resp.Content.Headers.ContentType?.MediaType ?? "";
        var bodyPreview = ct.StartsWith("text/") || ct.Contains("json") || ct.Contains("xml") || ct.Contains("html")
            ? System.Text.Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 4096))
            : $"(binary {bytes.Length} bytes, Content-Type={ct})";

        var exchange = new HttpExchange
        {
            Method = method,
            Url = url,
            RequestHeaders = reqHeaders,
            RequestBody = requestBody ?? "",
            StatusCode = (int)resp.StatusCode,
            ReasonPhrase = resp.ReasonPhrase ?? "",
            ResponseHeaders = FormatHeaders(resp.Headers, resp.Content.Headers),
            Body = bodyPreview,
            Elapsed = sw.Elapsed,
        };
        return (exchange, bytes);
    }

    private static string FormatHeaders(HttpHeaders? a, HttpHeaders? b)
    {
        var sb = new StringBuilder();
        if (a != null)
            foreach (var h in a)
                sb.Append(h.Key).Append(": ").AppendLine(string.Join(", ", h.Value));
        if (b != null)
            foreach (var h in b)
                sb.Append(h.Key).Append(": ").AppendLine(string.Join(", ", h.Value));
        return sb.ToString().TrimEnd();
    }
}
