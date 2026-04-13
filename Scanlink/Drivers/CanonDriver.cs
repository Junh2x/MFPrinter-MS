using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Drivers;

/// <summary>
/// 캐논 복합기 드라이버 — 박스 기능 사용.
/// 박스는 00~99 고정. 이름/비밀번호 설정으로 사용 여부 관리.
/// </summary>
public class CanonDriver : IMfpDriver
{
    public MfpBrand Brand => MfpBrand.Canon;

    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

    private static readonly int[] CanonMgmtPorts = [8000, 8443, 443, 80];

    private static string Dummy() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

    // ──────────────────────────────────────────────
    // 세션
    // ──────────────────────────────────────────────

    private static (HttpClient client, CookieContainer cookies) CreateClient()
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

    /// <summary>캐논 관리 포트 자동 탐색 → iR 쿠키 획득</summary>
    private static async Task<(HttpClient? client, string baseUrl, List<string> logs)> InitSessionAsync(MfpDevice device)
    {
        var logs = new List<string>();

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

        foreach (var baseUrl in portsToTry)
        {
            var (client, cookies) = CreateClient();
            try
            {
                logs.Add($"[세션] 포트 시도: {baseUrl}");
                var r = await client.GetAsync($"{baseUrl}/");
                if (!r.IsSuccessStatusCode)
                {
                    logs.Add($"[세션] {baseUrl} — HTTP {(int)r.StatusCode}");
                    client.Dispose();
                    continue;
                }

                var nativeUrl = $"{baseUrl}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={Dummy()}";
                var req = new HttpRequestMessage(HttpMethod.Get, nativeUrl);
                req.Headers.Add("Referer", $"{baseUrl}/");
                await client.SendAsync(req);

                var allCookies = cookies.GetAllCookies();
                var hasIR = allCookies.Any(c => c.Name == "iR");
                if (hasIR)
                {
                    logs.Add($"[세션] iR 쿠키 획득: {baseUrl}");
                    device.BaseUrl = baseUrl;
                    return (client, baseUrl, logs);
                }

                logs.Add($"[세션] {baseUrl} — iR 쿠키 미발급");
                client.Dispose();
            }
            catch (Exception ex)
            {
                logs.Add($"[세션] {baseUrl} — 실패: {ex.Message}");
                client.Dispose();
            }
        }

        logs.Add("[세션][FAIL] 모든 포트 실패");
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

    /// <summary>박스 목록 HTML 파싱 → List of (boxNo, name, docCount)</summary>
    private static List<(string boxNo, string name, int docCount)> ParseBoxList(string html)
    {
        var boxes = new List<(string, string, int)>();
        // 패턴: <span class="BoxNumber"><a href="javascript:box_documents('NN')">NN</a></span></td><td>NAME</td><td>COUNT</td>
        var rx = new Regex(
            @"BoxNumber[^<]*<a[^>]*box_documents\('(\d{2})'\)[^>]*>\d{2}</a></span></td>\s*<td>([^<]*)</td>\s*<td>(\d+)</td>",
            RegexOptions.Singleline);
        foreach (Match m in rx.Matches(html))
        {
            boxes.Add((m.Groups[1].Value, m.Groups[2].Value.Trim(), int.Parse(m.Groups[3].Value)));
        }
        return boxes;
    }

    /// <summary>박스 목록 조회</summary>
    private static async Task<(List<(string boxNo, string name, int docCount)> boxes, string html)>
        GetBoxListAsync(HttpClient client, string baseUrl)
    {
        var url = $"{baseUrl}/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox&Dummy={Dummy()}";
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Referer", $"{baseUrl}/rps/nativetop.cgi");
        var html = await (await client.SendAsync(req)).Content.ReadAsStringAsync();
        return (ParseBoxList(html), html);
    }

    /// <summary>박스 속성 페이지 (bprop.cgi) → Token 획득</summary>
    private static async Task<string?> GetBoxPropTokenAsync(HttpClient client, string baseUrl, string boxNo)
    {
        var url = $"{baseUrl}/rps/bprop.cgi?BOX_No={boxNo}&Dummy={Dummy()}";
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Referer", $"{baseUrl}/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox");
        var html = await (await client.SendAsync(req)).Content.ReadAsStringAsync();
        return ExtractTokenFromHidden(html);
    }

    /// <summary>박스 속성 업데이트 (이름/비밀번호)</summary>
    private static async Task<string> SetBoxPropAsync(
        HttpClient client, string baseUrl, string boxNo, string name, string password, string token)
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

        var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/rps/bpropset.cgi")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        req.Headers.Add("Referer", $"{baseUrl}/rps/bprop.cgi?");
        req.Headers.Add("Origin", baseUrl);
        var resp = await client.SendAsync(req);
        return await resp.Content.ReadAsStringAsync();
    }

    // ──────────────────────────────────────────────
    // ConnectAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult> ConnectAsync(MfpDevice device)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try
        {
            string baseUrl; List<string> logs;
            (client, baseUrl, logs) = await InitSessionAsync(device);
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
            return DriverResult.Fail($"연결 오류: {ex.Message}", result.Logs);
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // SetupAsync — 박스 방식은 별도 설정 불필요
    // ──────────────────────────────────────────────

    public Task<DriverResult> SetupAsync(MfpDevice device)
    {
        device.IsConfigured = true;
        return Task.FromResult(DriverResult.Ok("캐논 박스는 별도 초기 설정 불필요"));
    }

    // ──────────────────────────────────────────────
    // GetScanBoxListAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device)
    {
        var result = new DriverResult<List<ScanBox>> { Logs = [] };
        HttpClient? client = null;
        try
        {
            string baseUrl; List<string> logs;
            (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult<List<ScanBox>>.Fail("세션 실패", result.Logs);

            var (boxes, _) = await GetBoxListAsync(client, baseUrl);
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
            result.Success = false;
            result.Message = $"조회 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // AddScanBoxAsync — 빈 박스에 이름/비밀번호 설정
    // ──────────────────────────────────────────────

    public async Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try
        {
            result.Logs.Add($"[추가] 박스 추가: {box.Name}");

            string baseUrl; List<string> logs;
            (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            // 빈 박스 찾기
            var (boxes, _) = await GetBoxListAsync(client, baseUrl);
            if (boxes.Count == 0)
            {
                result.Logs.Add("[추가][FAIL] 박스 목록 파싱 실패");
                return DriverResult.Fail("박스 목록을 읽을 수 없습니다.", result.Logs);
            }

            var emptyBox = boxes.FirstOrDefault(b => string.IsNullOrWhiteSpace(b.name));
            if (emptyBox.boxNo == null)
            {
                result.Logs.Add("[추가][FAIL] 빈 박스 없음 (100개 모두 사용 중)");
                return DriverResult.Fail("빈 박스가 없습니다.", result.Logs);
            }

            // 이름 중복 확인
            if (boxes.Any(b => b.name == box.Name))
            {
                result.Logs.Add($"[추가][FAIL] '{box.Name}' 이름 중복");
                return DriverResult.Fail($"'{box.Name}' 박스가 이미 존재합니다.", result.Logs);
            }

            result.Logs.Add($"[추가] 빈 박스 선택: {emptyBox.boxNo}");

            // 박스 속성 페이지 → Token
            var token = await GetBoxPropTokenAsync(client, baseUrl, emptyBox.boxNo);
            if (token == null)
            {
                result.Logs.Add("[추가][FAIL] Token 획득 실패");
                return DriverResult.Fail("Token 획득 실패", result.Logs);
            }
            result.Logs.Add($"[추가] Token = {token[..Math.Min(15, token.Length)]}...");

            // 박스 속성 설정 (이름+비밀번호)
            var pw = box.Password ?? "";
            var respHtml = await SetBoxPropAsync(client, baseUrl, emptyBox.boxNo, box.Name, pw, token);
            result.Logs.Add($"[추가] 설정 응답: {respHtml.Length}자");

            if (respHtml.Contains("ERR") && !respHtml.Contains("ERR_SUBMIT_FORM"))
            {
                result.Logs.Add("[추가][FAIL] 서버 에러 응답");
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
            result.Success = false;
            result.Message = $"추가 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // UpdateScanBoxAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try
        {
            result.Logs.Add($"[수정] 박스 수정: {oldName ?? box.Name} → {box.Name} (슬롯={box.SlotIndex})");

            if (box.SlotIndex < 0)
                return DriverResult.Fail("박스 번호가 없습니다.", result.Logs);

            string baseUrl; List<string> logs;
            (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            var boxNo = box.SlotIndex.ToString("D2");

            var token = await GetBoxPropTokenAsync(client, baseUrl, boxNo);
            if (token == null)
                return DriverResult.Fail("Token 획득 실패", result.Logs);
            result.Logs.Add($"[수정] Token = {token[..Math.Min(15, token.Length)]}...");

            var pw = box.Password ?? "";
            var respHtml = await SetBoxPropAsync(client, baseUrl, boxNo, box.Name, pw, token);
            result.Logs.Add($"[수정] 응답: {respHtml.Length}자");

            if (respHtml.Contains("ERR") && !respHtml.Contains("ERR_SUBMIT_FORM"))
            {
                result.Logs.Add("[수정][FAIL] 서버 에러 응답");
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
            result.Success = false;
            result.Message = $"수정 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // GetBoxFilesAsync — 박스 내 파일 목록 조회
    // ──────────────────────────────────────────────

    /// <summary>박스 내 파일 목록 HTML 파싱</summary>
    private static List<BoxFile> ParseBoxFiles(string html)
    {
        var files = new List<BoxFile>();

        // <a href="javascript:doc_pages('NNNN')">...NAME...</a>
        // 그 뒤로 <td>SIZE...</td><td>PAGES</td><td>DATE</td>
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

    /// <summary>박스 내 파일 목록 조회</summary>
    public async Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult<List<BoxFile>> { Logs = [] };
        HttpClient? client = null;
        try
        {
            result.Logs.Add($"[파일목록] 박스 {box.SlotIndex}: {box.Name}");

            if (box.SlotIndex < 0)
                return DriverResult<List<BoxFile>>.Fail("박스 번호가 없습니다.", result.Logs);

            string baseUrl; List<string> logs;
            (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult<List<BoxFile>>.Fail("세션 실패", result.Logs);

            var boxNo = box.SlotIndex.ToString("D2");

            // 박스 로그인 (blogin.cgi) — 비밀번호 있으면 인증 필요
            if (!string.IsNullOrEmpty(box.Password))
            {
                result.Logs.Add("[파일목록] 박스 로그인 중...");
                var loginBody = $"BOX_No={boxNo}&Password={Uri.EscapeDataString(box.Password)}&Dummy={Dummy()}";
                var loginReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/rps/blogin.cgi")
                {
                    Content = new StringContent(loginBody, Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                loginReq.Headers.Add("Referer", $"{baseUrl}/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox");
                await client.SendAsync(loginReq);
            }

            // 파일 목록
            var body = $"BOX_No={boxNo}&DocStart=1&DIDS=&Dummy={Dummy()}";
            var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/rps/bcomdocs.cgi")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            req.Headers.Add("Referer", $"{baseUrl}/rps/blogin.cgi");
            var html = await (await client.SendAsync(req)).Content.ReadAsStringAsync();
            result.Logs.Add($"[파일목록] 응답: {html.Length}자");

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
            result.Success = false;
            result.Message = $"파일 목록 조회 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // DownloadBoxFileAsync — 박스 내 파일 다운로드 (JPG)
    // ──────────────────────────────────────────────

    /// <summary>박스 내 파일의 한 페이지를 JPG 바이트로 다운로드</summary>
    public async Task<DriverResult<byte[]>> DownloadBoxFilePageAsync(
        MfpDevice device, ScanBox box, string docId, int pageNo)
    {
        var result = new DriverResult<byte[]> { Logs = [] };
        HttpClient? client = null;
        try
        {
            string baseUrl; List<string> logs;
            (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult<byte[]>.Fail("세션 실패", result.Logs);

            var boxNo = box.SlotIndex.ToString("D2");

            // 박스 로그인
            if (!string.IsNullOrEmpty(box.Password))
            {
                var loginBody = $"BOX_No={boxNo}&Password={Uri.EscapeDataString(box.Password)}&Dummy={Dummy()}";
                var loginReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/rps/blogin.cgi")
                {
                    Content = new StringContent(loginBody, Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                loginReq.Headers.Add("Referer", $"{baseUrl}/rps/bpbl.cgi?CorePGTAG=16&BoxKind=UserBox");
                await client.SendAsync(loginReq);
            }

            var url = $"{baseUrl}/rps/image.jpg?BOX_No={boxNo}&DocID={docId}&PageNo={pageNo}&Mode=PJPEG&EFLG=true&Dummy={Dummy()}";
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("Referer", $"{baseUrl}/rps/bcomdocs.cgi");
            var resp = await client.SendAsync(req);

            if (!resp.IsSuccessStatusCode)
                return DriverResult<byte[]>.Fail($"다운로드 실패: HTTP {(int)resp.StatusCode}", result.Logs);

            var bytes = await resp.Content.ReadAsByteArrayAsync();
            result.Logs.Add($"[다운로드] {docId} 페이지 {pageNo}: {bytes.Length}바이트");

            result.Success = true;
            result.Data = bytes;
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[다운로드][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"다운로드 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // DeleteScanBoxAsync — 박스 이름/비밀번호 초기화
    // ──────────────────────────────────────────────

    public async Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try
        {
            result.Logs.Add($"[삭제] 박스 초기화: {box.Name} (슬롯={box.SlotIndex})");

            if (box.SlotIndex < 0)
                return DriverResult.Fail("박스 번호가 없습니다.", result.Logs);

            string baseUrl; List<string> logs;
            (client, baseUrl, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            var boxNo = box.SlotIndex.ToString("D2");

            var token = await GetBoxPropTokenAsync(client, baseUrl, boxNo);
            if (token == null)
                return DriverResult.Fail("Token 획득 실패", result.Logs);
            result.Logs.Add($"[삭제] Token = {token[..Math.Min(15, token.Length)]}...");

            // 이름/비밀번호 빈값으로 초기화
            var respHtml = await SetBoxPropAsync(client, baseUrl, boxNo, "", "", token);
            result.Logs.Add($"[삭제] 응답: {respHtml.Length}자");

            if (respHtml.Contains("ERR") && !respHtml.Contains("ERR_SUBMIT_FORM"))
            {
                result.Logs.Add("[삭제][FAIL] 서버 에러 응답");
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
            result.Success = false;
            result.Message = $"삭제 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }
}
