using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Drivers.Canon;

/// <summary>
/// 캐논 복합기 기본 드라이버 — 현재 대부분의 캐논 모델(iR-ADV 등)에 적용되는 플로우.
/// 박스는 00~99 고정. 이름/비밀번호 설정으로 사용 여부 관리.
/// </summary>
public sealed class CanonDefaultDriver : CanonDriverBase
{
    private static readonly int[] CanonMgmtPorts = [8000, 8443, 443, 80];

    // ──────────────────────────────────────────────
    // 세션 캐시 (기기별, TTL 없이 실패 전까지 재사용)
    // ──────────────────────────────────────────────

    private sealed class CanonSession
    {
        public required HttpClient Client { get; init; }
        public required string BaseUrl { get; init; }
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, CanonSession> _sessions = new();

    public override void DisposeSessions()
    {
        foreach (var ip in _sessions.Keys.ToList())
            InvalidateSession(ip);
    }

    /// <summary>캐논 서버에 명시적 로그아웃 + 로컬 세션 제거</summary>
    private static void InvalidateSession(string deviceIp)
    {
        if (!_sessions.TryRemove(deviceIp, out var old)) return;
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"{old.BaseUrl}/rps/logout.cgi?Dummy={Dummy()}");
            _ = old.Client.SendAsync(req);
        }
        catch { /* 로그아웃 실패해도 무시 */ }
        old.Client.Dispose();
    }

    // ──────────────────────────────────────────────
    // 세션
    // ──────────────────────────────────────────────

    /// <summary>캐논 관리 포트 자동 탐색 → iR 쿠키 획득. 캐시된 세션 우선 재사용.</summary>
    private static async Task<(HttpClient? client, string baseUrl, List<string> logs)> InitSessionAsync(MfpDevice device)
    {
        var logs = new List<string>();

        if (_sessions.TryGetValue(device.Ip, out var cached))
        {
            cached.LastUsed = DateTime.UtcNow;
            return (cached.Client, cached.BaseUrl, logs);
        }

        var portsToTry = new List<string>();
        if (!string.IsNullOrEmpty(device.BaseUrl))
            portsToTry.Add(device.BaseUrl);

        foreach (var port in CanonMgmtPorts)
        {
            var scheme = port is 443 or 8443 ? "https" : "http";
            var url = port is 80 or 443 ? $"{scheme}://{device.Ip}" : $"{scheme}://{device.Ip}:{port}";
            if (!portsToTry.Contains(url))
                portsToTry.Add(url);
        }

        HttpExchange? lastFail = null;
        foreach (var baseUrl in portsToTry)
        {
            var (client, cookies) = CreateClient();
            try
            {
                logs.Add($"[세션] 포트 시도: {baseUrl}");
                var rootEx = await GetAsync(client, $"{baseUrl}/");
                if (!rootEx.IsSuccessStatusCode)
                {
                    logs.Add($"[세션] {baseUrl} — HTTP {rootEx.StatusCode}");
                    lastFail = rootEx;
                    client.Dispose();
                    continue;
                }

                var nativeUrl = $"{baseUrl}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={Dummy()}";
                var nativeEx = await GetAsync(client, nativeUrl, $"{baseUrl}/");

                var allCookies = cookies.GetAllCookies();
                var hasIR = allCookies.Any(c => c.Name == "iR");
                if (hasIR)
                {
                    logs.Add($"[세션] iR 쿠키 획득: {baseUrl}");
                    device.BaseUrl = baseUrl;
                    _sessions[device.Ip] = new CanonSession { Client = client, BaseUrl = baseUrl };
                    return (client, baseUrl, logs);
                }

                logs.Add($"[세션] {baseUrl} — iR 쿠키 미발급 (nativetop HTTP {nativeEx.StatusCode})");
                lastFail = nativeEx;
                client.Dispose();
            }
            catch (Exception ex)
            {
                logs.Add($"[세션] {baseUrl} — 실패: {ex.Message}");
                logs.Add($"[STACK] {ex.StackTrace}");
                client.Dispose();
            }
        }

        logs.Add("[세션][FAIL] 모든 포트 실패");
        if (lastFail != null)
        {
            logs.Add("[세션][FAIL] 마지막 실패 응답 덤프:");
            logs.Add(lastFail.Dump());
        }
        return (null, "", logs);
    }

    // ──────────────────────────────────────────────
    // 박스 파싱/헬퍼
    // ──────────────────────────────────────────────

    private static string? ExtractTokenFromHidden(string html)
    {
        var m = Regex.Match(html, @"name=[""']Token[""'][^>]*value=[""']([^""']+)");
        return m.Success ? m.Groups[1].Value : null;
    }

    private static List<(string boxNo, string name, int docCount)> ParseBoxList(string html)
    {
        var boxes = new List<(string, string, int)>();
        var rx = new Regex(
            @"BoxNumber[^<]*<a[^>]*box_documents\('(\d{2})'\)[^>]*>\d{2}</a></span></td>\s*<td>([^<]*)</td>\s*<td>(\d+)</td>",
            RegexOptions.Singleline);
        foreach (Match m in rx.Matches(html))
        {
            boxes.Add((m.Groups[1].Value, m.Groups[2].Value.Trim(), int.Parse(m.Groups[3].Value)));
        }
        return boxes;
    }

    private static async Task<(List<(string boxNo, string name, int docCount)> boxes, HttpExchange ex)>
        GetBoxListAsync(HttpClient client, string baseUrl, List<string>? logs = null)
    {
        var url = $"{baseUrl}/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox&Dummy={Dummy()}";
        var ex = await GetAsync(client, url, $"{baseUrl}/rps/nativetop.cgi", logs);
        return (ParseBoxList(ex.Body), ex);
    }

    private static async Task<HttpExchange> BoxLoginAsync(HttpClient client, string baseUrl, string boxNo, string password, List<string>? logs = null)
    {
        string url, body, referer;
        if (string.IsNullOrEmpty(password))
        {
            url = $"{baseUrl}/rps/blogin.cgi";
            body = $"BOX_No={boxNo}&BoxKind=UserBox&Dummy={Dummy()}&Cookie=";
            referer = $"{baseUrl}/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox&FromTopPage=1&Dummy={Dummy()}";
        }
        else
        {
            url = $"{baseUrl}/rps/blogin.cgi?";
            body = $"BOX_No={boxNo}&DocID=&PgStart=&PIDS=&Password={Uri.EscapeDataString(password)}" +
                   $"&URLDirect=&BoxKind=UserBox&CorePGTAG=16&Dummy={Dummy()}";
            referer = $"{baseUrl}/rps/blogin.cgi";
        }

        return await PostFormAsync(client, url, body, referer, logs, baseUrl);
    }

    private static async Task<(string? token, HttpExchange? loginEx, HttpExchange propEx)> GetBoxPropTokenAsync(
        HttpClient client, string baseUrl, string boxNo, string password = "", List<string>? logs = null)
    {
        var loginEx = await BoxLoginAsync(client, baseUrl, boxNo, password, logs);

        var propBody = $"BOX_No={boxNo}&Dummy={Dummy()}";
        var propEx = await PostFormAsync(client, $"{baseUrl}/rps/bprop.cgi", propBody, $"{baseUrl}/rps/blogin.cgi", logs, baseUrl);
        var html = propEx.Body;

        var token = ExtractTokenFromHidden(html);
        if (token == null)
        {
            var m = Regex.Match(html, @"Token[""'\s=]+(\d{10,})");
            if (m.Success) token = m.Groups[1].Value;
        }

        return (token, loginEx, propEx);
    }

    private static async Task<HttpExchange> SetBoxPropAsync(
        HttpClient client, string baseUrl, string boxNo, string name, string password, string token, List<string>? logs = null)
    {
        var pwStat = string.IsNullOrEmpty(password) ? "0" : "1";
        var body = $"BOX_No={boxNo}" +
            $"&BoxName={Uri.EscapeDataString(name)}" +
            $"&PaswdStat={pwStat}&PswdChk={pwStat}" +
            $"&Password={Uri.EscapeDataString(password)}" +
            $"&RePassword={Uri.EscapeDataString(password)}" +
            $"&URLAdrStat=false&URLAdrID=0&PrtDrvSave=0" +
            $"&CoreNxAction=.%2Fbprop.cgi&CoreIncPartPg=" +
            $"&Dummy={Dummy()}" +
            "&COMADR_BNo_Reload=&COMADR_CNo_Reload=&COMADR_INo_Reload=&COMADR_PNo_Reload=" +
            "&COMADR_TargetAdrIDs_Reload=&COMADR_SubAdrStat_Reload=&COMADR_RtnCGI_Reload=" +
            "&COMADR_RemoteAdrs_Reload=&COMADR_RemoteAdrsStat_Reload=&COMADR_Tpl_Reload=" +
            "&COMADR_All_Reload=&COMADR_Group_Reload=&COMADR_Mail_Reload=&COMADR_G3Fax_Reload=" +
            "&COMADR_IFax_Reload=&COMADR_Printer_Reload=&COMADR_File_Reload=&COMADR_DB_Reload=" +
            "&COMADR_WebDav_Reload=&COMADR_AirFaxFlg_Reload=&URLAdrID_Reload=" +
            $"&Token={token}";

        return await PostFormAsync(client, $"{baseUrl}/rps/bpropset.cgi", body, $"{baseUrl}/rps/bprop.cgi?", logs, baseUrl);
    }

    // ──────────────────────────────────────────────
    // ConnectAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> ConnectAsync(MfpDevice device)
    {
        var result = new DriverResult();
        try
        {
            var (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);

            if (client == null)
                return DriverResult.Fail("연결 실패 — 캐논 관리 포트를 찾을 수 없습니다.", result.Logs);

            device.Status = ConnectionStatus.Connected;
            result.Success = true;
            result.Message = "연결 성공";
            result.Logs.Add($"[연결] {device.Ip} 관리 URL: {baseUrl}");
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[연결][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            return DriverResult.Fail($"연결 오류: {ex.Message}", result.Logs);
        }
    }

    // ──────────────────────────────────────────────
    // GetScanBoxListAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device)
    {
        var result = new DriverResult<List<ScanBox>> { Logs = [] };
        try
        {
            var (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult<List<ScanBox>>.Fail("세션 실패", result.Logs);

            var (boxes, listEx) = await GetBoxListAsync(client, baseUrl, result.Logs);
            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[조회][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult<List<ScanBox>>.Fail($"조회 HTTP {listEx.StatusCode}", result.Logs);
            }
            result.Logs.Add($"[조회] 전체 박스 {boxes.Count}개");

            var scanBoxes = boxes
                .Where(b => !string.IsNullOrWhiteSpace(b.name))
                .Select(b => new ScanBox
                {
                    Name = b.name,
                    SlotIndex = int.Parse(b.boxNo),
                    MfpDeviceId = device.Id,
                })
                .ToList();

            result.Success = true;
            result.Data = scanBoxes;
            result.Message = $"{scanBoxes.Count}개 조회 (사용 중)";
            result.Logs.Add($"[조회] 사용 중 박스 {scanBoxes.Count}개");
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[조회][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"조회 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // AddScanBoxAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        try
        {
            result.Logs.Add($"[추가] 박스 추가: {box.Name}");

            var (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            var (boxes, listEx) = await GetBoxListAsync(client, baseUrl, result.Logs);
            if (boxes.Count == 0)
            {
                result.Logs.Add("[추가][FAIL] 박스 목록 파싱 실패 — 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail("박스 목록을 읽을 수 없습니다.", result.Logs);
            }

            var emptyBox = boxes.FirstOrDefault(b => string.IsNullOrWhiteSpace(b.name));
            if (emptyBox.boxNo == null)
            {
                result.Logs.Add("[추가][FAIL] 빈 박스 없음 (100개 모두 사용 중)");
                return DriverResult.Fail("빈 박스가 없습니다.", result.Logs);
            }

            if (boxes.Any(b => b.name == box.Name))
            {
                result.Logs.Add($"[추가][FAIL] '{box.Name}' 이름 중복");
                return DriverResult.Fail($"'{box.Name}' 박스가 이미 존재합니다.", result.Logs);
            }

            result.Logs.Add($"[추가] 빈 박스 선택: {emptyBox.boxNo}");

            var (token, loginEx, propEx) = await GetBoxPropTokenAsync(client, baseUrl, emptyBox.boxNo, "", result.Logs);
            if (token == null)
            {
                result.Logs.Add("[추가][FAIL] Token 획득 실패 — 로그인/속성 응답 덤프:");
                if (loginEx != null) result.Logs.Add(loginEx.Dump());
                result.Logs.Add(propEx.Dump());
                return DriverResult.Fail("Token 획득 실패", result.Logs);
            }
            result.Logs.Add($"[추가] Token = {token[..Math.Min(15, token.Length)]}...");

            var pw = box.Password ?? "";
            var setEx = await SetBoxPropAsync(client, baseUrl, emptyBox.boxNo, box.Name, pw, token, result.Logs);

            if (!setEx.IsSuccessStatusCode || (setEx.Body.Contains("ERR") && !setEx.Body.Contains("ERR_SUBMIT_FORM")))
            {
                result.Logs.Add("[추가][FAIL] 서버 에러 응답 — 덤프:");
                result.Logs.Add(setEx.Dump());
                return DriverResult.Fail("박스 설정 실패", result.Logs);
            }

            box.SlotIndex = int.Parse(emptyBox.boxNo);
            result.Success = true;
            result.Message = "스캔함 추가 완료";
            result.Logs.Add($"[추가] 완료! 박스={emptyBox.boxNo}, 이름={box.Name}");
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[추가][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"추가 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // UpdateScanBoxAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null)
    {
        var result = new DriverResult();
        try
        {
            result.Logs.Add($"[수정] 박스 수정: {oldName ?? box.Name} → {box.Name} (슬롯={box.SlotIndex})");

            if (box.SlotIndex < 0)
                return DriverResult.Fail("박스 번호가 없습니다.", result.Logs);

            var (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            var boxNo = box.SlotIndex.ToString("D2");

            var (token, loginEx, propEx) = await GetBoxPropTokenAsync(client, baseUrl, boxNo, oldPassword ?? "", result.Logs);
            if (token == null)
            {
                result.Logs.Add("[수정][FAIL] Token 획득 실패 — 로그인/속성 응답 덤프:");
                if (loginEx != null) result.Logs.Add(loginEx.Dump());
                result.Logs.Add(propEx.Dump());
                return DriverResult.Fail("Token 획득 실패", result.Logs);
            }
            result.Logs.Add($"[수정] Token = {token[..Math.Min(15, token.Length)]}...");

            var pw = box.Password ?? "";
            var setEx = await SetBoxPropAsync(client, baseUrl, boxNo, box.Name, pw, token, result.Logs);

            if (!setEx.IsSuccessStatusCode || (setEx.Body.Contains("ERR") && !setEx.Body.Contains("ERR_SUBMIT_FORM")))
            {
                result.Logs.Add("[수정][FAIL] 서버 에러 응답 — 덤프:");
                result.Logs.Add(setEx.Dump());
                return DriverResult.Fail("박스 수정 실패", result.Logs);
            }

            result.Success = true;
            result.Message = "수정 완료";
            result.Logs.Add("[수정] 완료!");
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[수정][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"수정 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // GetBoxFilesAsync
    // ──────────────────────────────────────────────

    private static List<BoxFile> ParseBoxFiles(string html)
    {
        var files = new List<BoxFile>();

        var rx = new Regex(
            @"doc_pages\('(\d+)'\)[^>]*>\s*([^<]+)\s*</a>\s*</td>\s*<td>\s*([^<\s]+)[^<]*(?:<img[^>]*>)?\s*</td>\s*<td>\s*(\d+)\s*</td>\s*<td>\s*([^<]+)\s*</td>",
            RegexOptions.Singleline);

        foreach (Match m in rx.Matches(html))
        {
            var file = new BoxFile
            {
                DocId = m.Groups[1].Value,
                Name = m.Groups[2].Value.Trim(),
                Size = m.Groups[3].Value.Trim(),
                PageCount = int.Parse(m.Groups[4].Value),
            };
            if (DateTime.TryParse(m.Groups[5].Value.Trim().Replace("/", "-"), out var dt))
                file.ScannedAt = dt;
            files.Add(file);
        }
        return files;
    }

    public override async Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult<List<BoxFile>> { Logs = [] };
        try
        {
            result.Logs.Add($"[파일목록] 박스 {box.SlotIndex}: {box.Name}");

            var (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult<List<BoxFile>>.Fail("세션 실패", result.Logs);

            string boxNo;
            if (box.SlotIndex >= 0)
            {
                boxNo = box.SlotIndex.ToString("D2");
            }
            else
            {
                result.Logs.Add("[파일목록] SlotIndex 없음, 이름으로 박스 번호 조회...");
                var (allBoxes, _) = await GetBoxListAsync(client, baseUrl, result.Logs);
                var found = allBoxes.FirstOrDefault(b => b.name == box.Name);
                if (found.boxNo == null)
                    return DriverResult<List<BoxFile>>.Fail($"'{box.Name}' 박스를 찾을 수 없습니다.", result.Logs);
                boxNo = found.boxNo;
                box.SlotIndex = int.Parse(boxNo);
                result.Logs.Add($"[파일목록] 박스 번호 찾음: {boxNo}");
            }

            result.Logs.Add("[파일목록] 박스 진입 중...");
            var loginEx = await BoxLoginAsync(client, baseUrl, boxNo, box.Password ?? "", result.Logs);
            if (!loginEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[파일목록][FAIL] 박스 로그인 실패 — 덤프:");
                result.Logs.Add(loginEx.Dump());
                return DriverResult<List<BoxFile>>.Fail("박스 로그인 실패", result.Logs);
            }

            var body = $"BOX_No={boxNo}&DocStart=1&DIDS=&Dummy={Dummy()}";
            var docsEx = await PostFormAsync(client, $"{baseUrl}/rps/bcomdocs.cgi", body, $"{baseUrl}/rps/blogin.cgi", result.Logs);
            if (!docsEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[파일목록][FAIL] bcomdocs.cgi HTTP 실패 — 덤프:");
                result.Logs.Add(docsEx.Dump());
                return DriverResult<List<BoxFile>>.Fail($"HTTP {docsEx.StatusCode}", result.Logs);
            }
            var html = docsEx.Body;

            var boxNoMatch = Regex.Match(html, @"BOX_No[""']?[\s:=]+[""']?(\d{2})");
            if (boxNoMatch.Success && boxNoMatch.Groups[1].Value != boxNo)
            {
                result.Logs.Add($"[파일목록][WARN] 응답의 박스 번호 불일치: 요청={boxNo}, 응답={boxNoMatch.Groups[1].Value} — 덤프:");
                result.Logs.Add(docsEx.Dump());
                return DriverResult<List<BoxFile>>.Fail("박스 번호 불일치", result.Logs);
            }

            var files = ParseBoxFiles(html);
            result.Logs.Add($"[파일목록] 파일 {files.Count}개");

            result.Success = true;
            result.Data = files;
            result.Message = $"{files.Count}개 파일";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[파일목록][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            InvalidateSession(device.Ip);
            result.Success = false;
            result.Message = $"파일 목록 조회 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // DownloadBoxFilePageAsync — 박스 내 파일 페이지 단위 JPG 다운로드
    // (IMfpDriver 인터페이스 외: 캐논 전용 API)
    // ──────────────────────────────────────────────

    public async Task<DriverResult<byte[]>> DownloadBoxFilePageAsync(
        MfpDevice device, ScanBox box, string docId, int pageNo)
    {
        var result = new DriverResult<byte[]> { Logs = [] };
        try
        {
            var (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult<byte[]>.Fail("세션 실패", result.Logs);

            var boxNo = box.SlotIndex.ToString("D2");
            var loginEx = await BoxLoginAsync(client, baseUrl, boxNo, box.Password ?? "", result.Logs);
            if (!loginEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[다운로드][FAIL] 박스 로그인 실패"); result.Logs.Add(loginEx.Dump());
                return DriverResult<byte[]>.Fail("박스 로그인 실패", result.Logs);
            }

            var url = $"{baseUrl}/rps/image.jpg?BOX_No={boxNo}&DocID={docId}&PageNo={pageNo}&Mode=PJPEG&EFLG=true&Dummy={Dummy()}";
            var (imgEx, bytes) = await GetBytesAsync(client, url, $"{baseUrl}/rps/bcomdocs.cgi", result.Logs);

            if (!imgEx.IsSuccessStatusCode)
            {
                result.Logs.Add($"[다운로드][FAIL] HTTP {imgEx.StatusCode} — 덤프:");
                result.Logs.Add(imgEx.Dump());
                return DriverResult<byte[]>.Fail($"다운로드 실패: HTTP {imgEx.StatusCode}", result.Logs);
            }

            result.Logs.Add($"[다운로드] {docId} 페이지 {pageNo}: {bytes.Length}바이트");

            result.Success = true;
            result.Data = bytes;
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[다운로드][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"다운로드 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // DeleteScanBoxAsync — 박스 이름/비밀번호 초기화
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        try
        {
            result.Logs.Add($"[삭제] 박스 초기화: {box.Name} (슬롯={box.SlotIndex})");

            if (box.SlotIndex < 0)
                return DriverResult.Fail("박스 번호가 없습니다.", result.Logs);

            var (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            var boxNo = box.SlotIndex.ToString("D2");

            var (token, loginEx, propEx) = await GetBoxPropTokenAsync(client, baseUrl, boxNo, box.Password ?? "", result.Logs);
            if (token == null)
            {
                result.Logs.Add("[삭제][FAIL] Token 획득 실패 — 로그인/속성 덤프:");
                if (loginEx != null) result.Logs.Add(loginEx.Dump());
                result.Logs.Add(propEx.Dump());
                return DriverResult.Fail("Token 획득 실패", result.Logs);
            }
            result.Logs.Add($"[삭제] Token = {token[..Math.Min(15, token.Length)]}...");

            var setEx = await SetBoxPropAsync(client, baseUrl, boxNo, "", "", token, result.Logs);

            if (!setEx.IsSuccessStatusCode || (setEx.Body.Contains("ERR") && !setEx.Body.Contains("ERR_SUBMIT_FORM")))
            {
                result.Logs.Add("[삭제][FAIL] 서버 에러 응답 — 덤프:");
                result.Logs.Add(setEx.Dump());
                return DriverResult.Fail("박스 초기화 실패", result.Logs);
            }

            result.Success = true;
            result.Message = "삭제 완료";
            result.Logs.Add("[삭제] 완료! (박스는 남아있고 이름/비밀번호만 초기화됨)");
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[삭제][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"삭제 오류: {ex.Message}";
            return result;
        }
    }
}
