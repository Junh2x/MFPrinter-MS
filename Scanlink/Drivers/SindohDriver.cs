using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Drivers;

/// <summary>
/// 신도리코 복합기 드라이버.
/// JSON REST API (/wcd/api/) 기반 박스 CRUD.
/// </summary>
public class SindohDriver : IMfpDriver
{
    public MfpBrand Brand => MfpBrand.Sindoh;

    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36";

    // ──────────────────────────────────────────────
    // 세션 캐시 (기기별 재사용)
    // ──────────────────────────────────────────────

    private sealed class SindohSession
    {
        public required HttpClient Client { get; init; }
        public required string Token { get; set; }
        public required string BaseUrl { get; init; }
        public required CookieContainer Cookies { get; init; }
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SindohSession> _sessions = new();

    public static void DisposeAllSessions()
    {
        foreach (var ip in _sessions.Keys.ToList())
            InvalidateSession(ip);
    }

    private static void InvalidateSession(string deviceIp)
    {
        if (_sessions.TryRemove(deviceIp, out var old))
            old.Client.Dispose();
    }

    // ──────────────────────────────────────────────
    // 세션/토큰
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
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        return (client, cookies);
    }

    /// <summary>신도 웹 접속 → 세션 쿠키 + 토큰 획득. 캐시된 세션 우선 재사용.</summary>
    private static async Task<(HttpClient? client, string? token, string baseUrl, List<string> logs)>
        InitSessionAsync(MfpDevice device)
    {
        var logs = new List<string>();
        var baseUrl = string.IsNullOrEmpty(device.BaseUrl) ? $"http://{device.Ip}" : device.BaseUrl;

        // 캐시 재사용
        if (_sessions.TryGetValue(device.Ip, out var cached))
        {
            cached.LastUsed = DateTime.UtcNow;
            return (cached.Client, cached.Token, cached.BaseUrl, logs);
        }

        var (client, cookies) = CreateClient();
        var uri = new Uri(baseUrl);

        try
        {
            logs.Add($"[신도] 접속: {baseUrl}");

            // Step 1: 로그인 페이지 접속 (초기 쿠키 수집)
            await client.GetAsync($"{baseUrl}/wcd/spa_login.html");

            // Step 2: 공유 사용자 로그인 → ID 쿠키 획득
            logs.Add("[신도] 공유 사용자 로그인...");
            var loginPayload = "func=PSL_LP0_TOP&AuthType=None&TrackType=&ExtSvType=0&PswcForm=&Mode=Public" +
                "&publicuser=&username=&password=&AuthorityType=&R_ADM=&ExtServ=0&ViewMode=&BrowserMode=&Lang=" +
                "&trackname=&trackpassword=";
            var loginReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/wcd/ulogin.cgi")
            {
                Content = new StringContent(loginPayload, Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            loginReq.Headers.Add("Referer", $"{baseUrl}/wcd/spa_login.html");
            loginReq.Headers.Add("Accept", "*/*");
            var loginResp = await client.SendAsync(loginReq);
            var loginHtml = await loginResp.Content.ReadAsStringAsync();
            logs.Add($"[신도] 로그인 응답: {loginHtml.Length}자");

            // ID 쿠키 확인
            var allCookies = cookies.GetAllCookies();
            var idCookie = allCookies.FirstOrDefault(c => c.Name == "ID");
            if (idCookie == null || string.IsNullOrEmpty(idCookie.Value))
            {
                logs.Add("[신도][FAIL] 로그인 실패 — ID 쿠키 없음");
                client.Dispose();
                return (null, null, baseUrl, logs);
            }
            logs.Add($"[신도] 세션 ID: {idCookie.Value[..Math.Min(10, idCookie.Value.Length)]}...");

            // Step 3: 공유 사용자 API 라우팅 쿠키
            cookies.Add(uri, new Cookie("menuType", "Public"));
            cookies.Add(uri, new Cookie("usr", "F_ULU"));
            cookies.Add(uri, new Cookie("box_dsp", "Setting"));
            cookies.Add(uri, new Cookie("webUI", "new"));

            // Step 4: 메인 페이지 접속
            await client.GetAsync($"{baseUrl}/wcd/spa_main.html");

            // Step 5: 토큰 추출 — 박스 목록 조회
            var tokenResp = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "1" } }
                    },
                    Token = ""
                }, $"{baseUrl}/wcd/spa_main.html");

            logs.Add($"[신도] 토큰 조회: {tokenResp.Length}자");

            var tokenMatch = Regex.Match(tokenResp, @"""Token""\s*:\s*""([^""]+)""");
            string? token = tokenMatch.Success ? tokenMatch.Groups[1].Value : null;

            if (token == null)
            {
                logs.Add($"[신도][WARN] 토큰 미추출, 응답(앞300자): {tokenResp[..Math.Min(300, tokenResp.Length)]}");
                token = "";
            }
            else
            {
                logs.Add($"[신도] 토큰: {token[..Math.Min(10, token.Length)]}...");
            }

            device.BaseUrl = baseUrl;

            // 캐시에 저장 (실패 시까지 재사용)
            _sessions[device.Ip] = new SindohSession { Client = client, Token = token ?? "", BaseUrl = baseUrl, Cookies = cookies };

            return (client, token, baseUrl, logs);
        }
        catch (Exception ex)
        {
            logs.Add($"[신도][ERROR] 세션: {ex.Message}");
            client.Dispose();
            return (null, null, baseUrl, logs);
        }
    }

    private static async Task<string> PostJsonAsync(HttpClient client, string url, object data, string referer)
    {
        var json = JsonSerializer.Serialize(data);
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        req.Headers.Add("Referer", referer);
        req.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        var r = await client.SendAsync(req);
        return await r.Content.ReadAsStringAsync();
    }

    // ──────────────────────────────────────────────
    // ConnectAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult> ConnectAsync(MfpDevice device)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try {
        string? token; List<string> logs;
        (client, token, _, logs) = await InitSessionAsync(device);
        result.Logs.AddRange(logs);
        if (client == null) return DriverResult.Fail("연결 실패", result.Logs);
        device.Status = ConnectionStatus.Connected;
        result.Success = true;
        result.Message = "연결 성공";
        return result;
        } catch (Exception ex) {
            result.Logs.Add($"[신도][ERROR] {ex.Message}");
            return DriverResult.Fail($"연결 오류: {ex.Message}", result.Logs);
        } finally { /* 세션은 캐시에서 관리 */ }
    }

    public Task<DriverResult> SetupAsync(MfpDevice device)
    {
        device.IsConfigured = true;
        return Task.FromResult(DriverResult.Ok("신도는 별도 초기 설정 불필요"));
    }

    // ──────────────────────────────────────────────
    // GetScanBoxListAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device)
    {
        var result = new DriverResult<List<ScanBox>> { Logs = [] };
        HttpClient? client = null;
        try {
        string? token; string baseUrl; List<string> loginLogs;
        (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
        result.Logs.AddRange(loginLogs);
        if (client == null) return DriverResult<List<ScanBox>>.Fail("세션 실패", result.Logs);

        // 박스 목록 조회는 InitSession에서 이미 호출됨, 여기서는 전체 조회
        var resp = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
            new {
                BoxListCondition = new {
                    SearchKey = "None", WellUse = "false",
                    BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                    ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                },
                Token = token ?? ""
            }, $"{baseUrl}/wcd/spa_main.html");

        // 응답 파싱 (BoxID + Name)
        var boxes = new List<ScanBox>();
        foreach (Match m in Regex.Matches(resp, @"""BoxID""\s*:\s*""(\d+)"".*?""Name""\s*:\s*""([^""]+)""", RegexOptions.Singleline))
        {
            boxes.Add(new ScanBox { Name = m.Groups[2].Value, SlotIndex = int.Parse(m.Groups[1].Value), MfpDeviceId = device.Id });
        }

        result.Logs.Add($"[신도] 박스 {boxes.Count}개 조회");
        result.Success = true;
        result.Data = boxes;
        return result;
        } catch (Exception ex) {
            result.Logs.Add($"[신도][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"조회 오류: {ex.Message}";
            return result;
        } finally { /* 세션은 캐시에서 관리 */ }
    }

    // ──────────────────────────────────────────────
    // AddScanBoxAsync — 박스 생성
    // ──────────────────────────────────────────────

    public async Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try {

        result.Logs.Add($"[신도추가] 박스 생성: {box.Name}");

        string? token; string baseUrl; List<string> loginLogs;
        (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
        result.Logs.AddRange(loginLogs);
        if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

        var pw = box.Password ?? "";
        var usePass = !string.IsNullOrEmpty(pw) ? "UsePass" : "NoPass";

        var resp = await PostJsonAsync(client,
            $"{baseUrl}/wcd/api/AppReqSetCustomMessage/_005_001_ULU001",
            new {
                func = "PSL_F_ULUUser_CRE",
                h_token = token ?? "",
                H_TAB = "",
                H_GNA = "",
                R_NUM = "Space",
                T_NAM = box.Name,
                C_USE = usePass,
                P_PAS = pw,
                S_SER = "Abc",
                S_BTY = "Public",
                R_SAP = "None",
                S_GFC = "On",
                S_SEC = "Off",
                S_DTP = "false",
            }, $"{baseUrl}/wcd/spa_main.html");

        result.Logs.Add($"[신도추가] 응답: {resp.Length}자");
        result.Logs.Add($"[신도추가] 응답 내용(앞300자): {resp[..Math.Min(300, resp.Length)]}");

        if (!resp.Contains("\"Ack\"") && !resp.Contains("\"Ok_1\""))
        {
            result.Logs.Add($"[신도추가][FAIL] 생성 실패: {resp[..Math.Min(300, resp.Length)]}");
            return DriverResult.Fail($"박스 생성 실패", result.Logs);
        }

        result.Logs.Add("[신도추가] 생성 완료");
        result.Success = true;
        result.Message = "스캔함 추가 완료";
        return result;
        } catch (Exception ex) {
            result.Logs.Add($"[신도추가][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"추가 오류: {ex.Message}";
            return result;
        } finally { /* 세션은 캐시에서 관리 */ }
    }

    // ──────────────────────────────────────────────
    // UpdateScanBoxAsync — 박스 수정 (이름/비밀번호)
    // ──────────────────────────────────────────────

    public async Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try {

        var searchName = oldName ?? box.Name;
        result.Logs.Add($"[신도수정] 박스 수정: {searchName} → {box.Name}");

        string? token; string baseUrl; List<string> loginLogs;
        (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
        result.Logs.AddRange(loginLogs);
        if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

        // 박스 목록에서 대상 번호 찾기
        var listResp = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
            new {
                BoxListCondition = new {
                    SearchKey = "None", WellUse = "false",
                    BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                    ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                },
                Token = token ?? ""
            }, $"{baseUrl}/wcd/spa_main.html");

        // BoxNumber 찾기: "BoxName":"searchName" 근처의 "BoxNumber":"N"
        var boxNum = FindBoxNumber(listResp, searchName);
        if (boxNum == null)
        {
            result.Logs.Add($"[신도수정][FAIL] 박스 '{searchName}' 찾을 수 없음");
            return DriverResult.Fail($"박스 '{searchName}'을 찾을 수 없습니다.", result.Logs);
        }
        result.Logs.Add($"[신도수정] 대상 박스 번호: {boxNum}");

        var pw = box.Password ?? "";
        var changePw = !string.IsNullOrEmpty(pw);

        var resp = await PostJsonAsync(client,
            $"{baseUrl}/wcd/api/AppReqSetCustomMessage/_005_001_ULU002",
            new {
                func = "PSL_F_ULU_SET",
                h_token = token ?? "",
                H_TAB = "",
                T_NAM = box.Name,
                S_SER = "Abc",
                R_SAP = "None",
                H_BOX = boxNum,
                H_USR = "",
                H_BPA = "",
                H_BTY = "User",
                H_BAT = "Public",
                H_XTP = "",
                H_DSP = "Setting",
                C_PAC = changePw ? "true" : "false",
                P_CPA = oldPassword ?? "",
                P_NPA = pw,
                P_NPA2 = pw,
            }, $"{baseUrl}/wcd/spa_main.html");

        result.Logs.Add($"[신도수정] 응답: {resp.Length}자");

        if (!resp.Contains("\"Ack\"") && !resp.Contains("\"Ok_1\""))
        {
            result.Logs.Add($"[신도수정][FAIL] 수정 실패: {resp[..Math.Min(300, resp.Length)]}");
            return DriverResult.Fail("수정 실패", result.Logs);
        }

        result.Logs.Add("[신도수정] 수정 완료");
        result.Success = true;
        result.Message = "수정 완료";
        return result;
        } catch (Exception ex) {
            result.Logs.Add($"[신도수정][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"수정 오류: {ex.Message}";
            return result;
        } finally { /* 세션은 캐시에서 관리 */ }
    }

    // ──────────────────────────────────────────────
    // DeleteScanBoxAsync — 박스 삭제 (2단계)
    // ──────────────────────────────────────────────

    public async Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try {

        result.Logs.Add($"[신도삭제] 박스 삭제: {box.Name}");

        string? token; string baseUrl; List<string> loginLogs;
        (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
        result.Logs.AddRange(loginLogs);
        if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

        // 박스 번호 찾기
        var listResp = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
            new {
                BoxListCondition = new {
                    SearchKey = "None", WellUse = "false",
                    BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                    ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                },
                Token = token ?? ""
            }, $"{baseUrl}/wcd/spa_main.html");

        result.Logs.Add($"[신도삭제] 목록 응답: {listResp.Length}자");
        result.Logs.Add($"[신도삭제] 목록 내용(앞500자): {listResp[..Math.Min(500, listResp.Length)]}");

        var boxNum = FindBoxNumber(listResp, box.Name);
        if (boxNum == null)
        {
            result.Logs.Add($"[신도삭제][FAIL] 박스 '{box.Name}' 목록에서 찾을 수 없음");
            return DriverResult.Fail($"박스 '{box.Name}'을 찾을 수 없습니다.", result.Logs);
        }
        result.Logs.Add($"[신도삭제] 대상 박스 ID: {boxNum}");

        var pw = box.Password ?? "";
        var uri = new Uri(baseUrl);

        // Step 1: 새 토큰 발급
        result.Logs.Add("[신도삭제] Step1: 토큰 갱신...");
        var cookies = ((HttpClientHandler)typeof(HttpMessageInvoker)
            .GetField("_handler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(client)! as HttpClientHandler)!.CookieContainer;
        cookies.Add(uri, new Cookie("usr", "F_ULUUserBoxLogin"));

        var tokenResp = await (await client.GetAsync($"{baseUrl}/wcd/token.json?_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}")).Content.ReadAsStringAsync();
        var tokenMatch = Regex.Match(tokenResp, @"""Token""\s*:\s*""([^""]+)""");
        if (tokenMatch.Success) token = tokenMatch.Groups[1].Value;
        result.Logs.Add($"[신도삭제] 토큰: {token?[..Math.Min(10, token?.Length ?? 0)]}...");

        // Step 2: 비밀번호 인증 (form-urlencoded)
        result.Logs.Add("[신도삭제] Step2: 비밀번호 인증...");
        var authPayload = $"func=PSL_F_ULUUser_BOX&h_token={token}&H_TAB=&H_BID={boxNum}&H_DSP=Delete&H_IPA=On&T_BID={boxNum}&H_BAT=Public&P_BPA={Uri.EscapeDataString(pw)}";
        var authReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/wcd/api/AppReqSetCustomMessage/_005_001_ULU004")
        {
            Content = new StringContent(authPayload, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        authReq.Headers.Add("Referer", $"{baseUrl}/wcd/spa_main.html");
        authReq.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        var authResp = await (await client.SendAsync(authReq)).Content.ReadAsStringAsync();
        result.Logs.Add($"[신도삭제] Step2 응답: {authResp.Length}자");

        // Step 3: 실제 삭제
        result.Logs.Add("[신도삭제] Step3: 삭제 확정...");
        var resp = await PostJsonAsync(client,
            $"{baseUrl}/wcd/api/AppReqSetCustomMessage/_005_001_ULU003",
            new {
                func = "PSL_F_ULU_DEL",
                h_token = token ?? "",
                H_XTP = "true",
                H_TAB = "",
                H_BOX = boxNum,
                H_USR = "",
                H_BPA = "",
                H_NAM = box.Name,
                H_SAV = "0",
                H_BTY = "User",
                H_DCNT = "0",
            }, $"{baseUrl}/wcd/spa_main.html");

        result.Logs.Add($"[신도삭제] Step3 응답: {resp.Length}자");

        if (!resp.Contains("\"Ack\"") && !resp.Contains("\"Ok_1\""))
        {
            result.Logs.Add($"[신도삭제][FAIL] 삭제 실패: {resp[..Math.Min(300, resp.Length)]}");
            return DriverResult.Fail("삭제 실패", result.Logs);
        }

        result.Logs.Add("[신도삭제] 삭제 완료");
        result.Success = true;
        result.Message = "삭제 완료";
        return result;
        } catch (Exception ex) {
            result.Logs.Add($"[신도삭제][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"삭제 오류: {ex.Message}";
            return result;
        } finally { /* 세션은 캐시에서 관리 */ }
    }

    // ──────────────────────────────────────────────
    // 유틸
    // ──────────────────────────────────────────────

    /// <summary>
    /// usr 쿠키를 주어진 값으로 교체. 기존 usr 쿠키(path=/, path=/wcd 모두) 제거 후 새로 추가.
    /// 서버가 path=/wcd로 설정하기 때문에 path 중복을 피해야 함.
    /// </summary>
    private static void ReplaceUsrCookie(CookieContainer cookies, Uri baseUri, Uri wcdUri, string value)
    {
        // 기존 usr 쿠키 모두 만료
        foreach (Cookie c in cookies.GetCookies(wcdUri))
            if (c.Name == "usr") c.Expired = true;
        foreach (Cookie c in cookies.GetCookies(baseUri))
            if (c.Name == "usr") c.Expired = true;

        // path=/wcd 로 재설정 (서버와 동일)
        cookies.Add(wcdUri, new Cookie("usr", value, "/wcd"));
    }

    /// <summary>박스 목록 응답에서 이름으로 BoxID 찾기</summary>
    private static string? FindBoxNumber(string json, string boxName)
    {
        // "BoxID":"N" ... "Name":"name" 패턴
        var blocks = Regex.Matches(json, @"""BoxID""\s*:\s*""(\d+)"".*?""Name""\s*:\s*""([^""]+)""", RegexOptions.Singleline);
        foreach (Match m in blocks)
        {
            if (m.Groups[2].Value == boxName)
                return m.Groups[1].Value;
        }

        return null;
    }

    /// <summary>신도 박스 내 파일 목록 조회 (3단계)</summary>
    public async Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult<List<BoxFile>> { Logs = [] };
        try
        {
            result.Logs.Add($"[신도파일] 박스 '{box.Name}' 파일 목록 조회");

            HttpClient? client; string? token; string baseUrl; List<string> loginLogs;
            (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null || token == null)
                return DriverResult<List<BoxFile>>.Fail("세션 실패", result.Logs);

            // 캐시된 쿠키 컨테이너 가져오기
            if (!_sessions.TryGetValue(device.Ip, out var session))
                return DriverResult<List<BoxFile>>.Fail("세션 캐시 없음", result.Logs);

            // 박스 번호 찾기
            var listResp = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                    },
                    Token = token
                }, $"{baseUrl}/wcd/spa_main.html");

            var boxNum = FindBoxNumber(listResp, box.Name);
            if (boxNum == null)
                return DriverResult<List<BoxFile>>.Fail($"박스 '{box.Name}' 찾을 수 없음", result.Logs);

            result.Logs.Add($"[신도파일] 박스 번호: {boxNum}");

            // Step 1: 파일 관리 쿠키로 전환 (F_ULU → F_UOU)
            var uri = new Uri(baseUrl);
            var wcdUri = new Uri($"{baseUrl}/wcd/");
            ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");
            result.Logs.Add("[신도파일] 쿠키 usr=F_UOU 설정");

            // Step 2: 박스 진입 (_105_000_ULU000) — 비밀번호 인증
            var pw = box.Password ?? "";
            result.Logs.Add($"[신도파일] 박스 진입 (비밀번호: {(string.IsNullOrEmpty(pw) ? "없음" : "있음")})");
            var enterResp = await PostJsonAsync(client,
                $"{baseUrl}/wcd/api/AppReqSetCustomMessage/_105_000_ULU000",
                new {
                    func = "PSL_F_UOUUser_BOX",
                    H_BID = boxNum, T_BID = boxNum, P_BPA = pw,
                    h_token = token, H_PID = "-1", H_IPA = "On",
                    H_BAT = "Public", _ = "",
                }, $"{baseUrl}/wcd/spa_main.html");
            result.Logs.Add($"[신도파일] 진입 응답: {enterResp.Length}자 — 앞200자: {enterResp[..Math.Min(200, enterResp.Length)]}");

            // Step 3-A: box_detail.json 최초 요청 → waitmsg 엔드포인트로 polling 시작
            var detailReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/wcd/box_detail.json")
            {
                Content = new StringContent("waitend=true&TaskNo=0&_=", Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            detailReq.Headers.Add("Referer", $"{baseUrl}/wcd/spa_main.html");
            detailReq.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            var firstResp = await (await client.SendAsync(detailReq)).Content.ReadAsStringAsync();
            result.Logs.Add($"[신도파일] box_detail.json 초기 응답: {firstResp.Length}자");

            var taskNo = "0";
            var taskMatch = Regex.Match(firstResp, @"""ParamValue""\s*:\s*""([^""]+)""");
            if (taskMatch.Success) taskNo = taskMatch.Groups[1].Value;

            // Step 3-B: /wcd/waitmsg polling
            string detailResp = firstResp;
            var ready = false;
            for (var attempt = 0; attempt < 30; attempt++)
            {
                var waitReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/wcd/waitmsg")
                {
                    Content = new StringContent($"TaskNo={taskNo}&_=", Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                waitReq.Headers.Add("Referer", $"{baseUrl}/wcd/spa_main.html");
                waitReq.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                var waitResp = await (await client.SendAsync(waitReq)).Content.ReadAsStringAsync();
                result.Logs.Add($"[신도파일] waitmsg {attempt + 1}: {waitResp.Length}자");

                // waitend=true → 데이터 준비 완료
                if (waitResp.Contains("\"waitend\":\"true\""))
                {
                    result.Logs.Add("[신도파일] waitend=true — 데이터 준비 완료");
                    ready = true;
                    break;
                }

                // 계속 wait → Interval만큼 대기
                var intervalMatch = Regex.Match(waitResp, @"""Interval""\s*:\s*""(\d+)""");
                var interval = intervalMatch.Success ? int.Parse(intervalMatch.Groups[1].Value) : 200;
                await Task.Delay(Math.Min(interval, 500));  // 최대 500ms 대기 제한
            }

            if (!ready)
            {
                result.Logs.Add("[신도파일] waitmsg 타임아웃");
                return DriverResult<List<BoxFile>>.Fail("데이터 준비 타임아웃", result.Logs);
            }

            // Step 3-C: 최종 box_detail.json 요청 — 실제 데이터
            var finalReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/wcd/box_detail.json")
            {
                Content = new StringContent($"TaskNo={taskNo}&waitend=true", Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            finalReq.Headers.Add("Referer", $"{baseUrl}/wcd/spa_main.html");
            finalReq.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            detailResp = await (await client.SendAsync(finalReq)).Content.ReadAsStringAsync();
            result.Logs.Add($"[신도파일] 최종 box_detail.json: {detailResp.Length}자");

            // 응답 원본 저장 (디버깅)
            try
            {
                var debugDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "logs");
                System.IO.Directory.CreateDirectory(debugDir);
                var debugPath = System.IO.Path.Combine(debugDir, $"sindoh_box_detail_{box.Name}_{DateTime.Now:HHmmss}.json");
                await System.IO.File.WriteAllTextAsync(debugPath, detailResp);
                result.Logs.Add($"[신도파일] 응답 저장: {debugPath}");
            }
            catch (Exception ex)
            {
                result.Logs.Add($"[신도파일] 응답 저장 실패: {ex.Message}");
            }

            var boxJobIdx = detailResp.IndexOf("BoxJobInfo", StringComparison.OrdinalIgnoreCase);
            if (boxJobIdx >= 0)
            {
                var snippet = detailResp.Substring(boxJobIdx, Math.Min(500, detailResp.Length - boxJobIdx));
                result.Logs.Add($"[신도파일] BoxJobInfo 위치: {boxJobIdx}");
                result.Logs.Add($"[신도파일] BoxJobInfo 내용: {snippet}");
            }
            else
            {
                result.Logs.Add($"[신도파일] BoxJobInfo 없음. 앞500자: {detailResp[..Math.Min(500, detailResp.Length)]}");
            }

            // 응답의 박스 번호가 요청한 박스와 일치하는지 검증
            var actualBoxId = ExtractBoxId(detailResp);
            result.Logs.Add($"[신도파일] 응답의 BoxID={actualBoxId}, 요청={boxNum}");
            if (actualBoxId != null && actualBoxId != boxNum)
            {
                result.Logs.Add($"[신도파일][WARN] 박스 불일치 — 세션 오염, 무효화");
                InvalidateSession(device.Ip);
                return DriverResult<List<BoxFile>>.Fail($"박스 불일치 (응답={actualBoxId}, 요청={boxNum})", result.Logs);
            }

            // JSON 파싱
            var files = ParseSindohBoxFiles(detailResp);
            result.Logs.Add($"[신도파일] 파싱된 파일 {files.Count}개");
            foreach (var f in files)
                result.Logs.Add($"  - {f.Name} ({f.DocId})");

            result.Success = true;
            result.Data = files;
            result.Message = $"{files.Count}개 파일";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도파일][ERROR] {ex.Message}");
            result.Logs.Add($"[신도파일][STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"파일 목록 오류: {ex.Message}";
            return result;
        }
    }

    /// <summary>box_detail.json 응답에서 실제 박스 ID 추출 (Job.BoxInfoList.BoxInfo.BoxID)</summary>
    private static string? ExtractBoxId(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("MFP", out var mfp)
                && mfp.TryGetProperty("Job", out var job)
                && job.TryGetProperty("BoxInfoList", out var bil)
                && bil.TryGetProperty("BoxInfo", out var bi)
                && bi.TryGetProperty("BoxID", out var id))
            {
                return id.GetString();
            }
        }
        catch { }
        return null;
    }

    /// <summary>box_detail.json에서 파일 목록 파싱. Job.BoxInfoList.BoxJobInfoList.BoxJobInfo 경로</summary>
    private static List<BoxFile> ParseSindohBoxFiles(string json)
    {
        var files = new List<BoxFile>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("MFP", out var mfp)) return files;
            if (!mfp.TryGetProperty("Job", out var job)) return files;
            if (!job.TryGetProperty("BoxInfoList", out var boxInfoList)) return files;
            if (!boxInfoList.TryGetProperty("BoxJobInfoList", out var boxJobInfoList)) return files;
            if (!boxJobInfoList.TryGetProperty("BoxJobInfo", out var boxJobInfo)) return files;

            // 단일 객체 또는 배열
            if (boxJobInfo.ValueKind == JsonValueKind.Array)
            {
                foreach (var job2 in boxJobInfo.EnumerateArray())
                    AddFileFromJson(job2, files);
            }
            else if (boxJobInfo.ValueKind == JsonValueKind.Object)
            {
                AddFileFromJson(boxJobInfo, files);
            }
        }
        catch { /* 파싱 실패 시 빈 목록 */ }

        return files;
    }

    private static void AddFileFromJson(JsonElement job, List<BoxFile> files)
    {
        var file = new BoxFile
        {
            DocId = job.TryGetProperty("BoxJobID", out var id) ? (id.GetString() ?? "") : "",
            Name = job.TryGetProperty("JobName", out var n) ? (n.GetString() ?? "") : "",
            PageCount = 1,
            Size = "",
        };

        if (job.TryGetProperty("JobTime", out var jt)
            && jt.TryGetProperty("CreateTime", out var ct)
            && ct.TryGetProperty("Year", out var y)
            && ct.TryGetProperty("Month", out var mo)
            && ct.TryGetProperty("Day", out var d)
            && ct.TryGetProperty("Hour", out var h)
            && ct.TryGetProperty("Minute", out var mi))
        {
            try
            {
                file.ScannedAt = new DateTime(
                    int.Parse(y.GetString()!),
                    int.Parse(mo.GetString()!),
                    int.Parse(d.GetString()!),
                    int.Parse(h.GetString()!),
                    int.Parse(mi.GetString()!), 0);
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(file.DocId))
            files.Add(file);
    }

    // ──────────────────────────────────────────────
    // DownloadFileAsync — 박스 내 파일 PDF 다운로드
    // ──────────────────────────────────────────────

    public async Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file)
    {
        var result = new DriverResult<byte[]> { Logs = [] };
        try
        {
            result.Logs.Add($"[신도다운] 파일 다운로드: {file.Name} (박스={box.Name})");

            HttpClient? client; string? token; string baseUrl; List<string> loginLogs;
            (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null || token == null)
                return DriverResult<byte[]>.Fail("세션 실패", result.Logs);

            if (!_sessions.TryGetValue(device.Ip, out var session))
                return DriverResult<byte[]>.Fail("세션 캐시 없음", result.Logs);

            // 박스 번호 찾기
            var listResp = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                    },
                    Token = token
                }, $"{baseUrl}/wcd/spa_main.html");

            var boxNum = FindBoxNumber(listResp, box.Name);
            if (boxNum == null)
                return DriverResult<byte[]>.Fail($"박스 '{box.Name}' 찾을 수 없음", result.Logs);

            // Step 1: 쿠키 전환 F_UOU_FileDownload (기존 usr 쿠키 모두 만료 후 path=/wcd로 재설정)
            var uri = new Uri(baseUrl);
            var wcdUri = new Uri($"{baseUrl}/wcd/");
            ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU_FileDownload");
            result.Logs.Add("[신도다운] 쿠키 usr=F_UOU_FileDownload");

            // Step 2: 다운로드 요청 (_105_000_ULU004)
            var pw = box.Password ?? "";
            result.Logs.Add("[신도다운] 다운로드 준비 요청 (ULU004)...");
            var prepResp = await PostJsonAsync(client,
                $"{baseUrl}/wcd/api/AppReqSetCustomMessage/_105_000_ULU004",
                new {
                    H_TAB = "",
                    func = "PSL_F_UOU_DWN",
                    h_token = token,
                    H_BOX = boxNum,
                    H_BPA = pw,
                    H_BTY = "User",
                    H_PID = "-1",
                    H_DTY = "FileDownload",
                    H_XTP = "Thumbnail",
                    H_FMT = "CompactPdf",
                    H_JLS = "@1@",
                    H_JNL = $"\t{file.Name}\t",
                    H_JOR = "@1@",
                    H_DCN = "1",
                    C_GFA = "On",
                    F_UOU_S_FOR = "CompactPdf",
                    S_OUT = "Off",
                    S_LRP = "Off",
                    S_PDA = "Off",
                    F_UOU_R_PAG = "MultiPage",
                    R_SPG = "Off",
                }, $"{baseUrl}/wcd/spa_main.html");
            result.Logs.Add($"[신도다운] 준비 응답({prepResp.Length}자): {prepResp[..Math.Min(250, prepResp.Length)]}");

            // Step 3: 초기 Interval만큼 대기 후 progress 엔드포인트로 폴링 (본문 없음)
            var initIntervalMatch = Regex.Match(prepResp, @"""Interval""\s*:\s*""(\d+)""");
            var initInterval = initIntervalMatch.Success ? int.Parse(initIntervalMatch.Groups[1].Value) : 2500;
            await Task.Delay(Math.Min(initInterval, 2500));

            var ready = false;
            string? lastProg = null;
            for (var attempt = 0; attempt < 30; attempt++)
            {
                var progReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/wcd/progress")
                {
                    Content = new StringContent("_=", Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                progReq.Headers.Add("Referer", $"{baseUrl}/wcd/spa_main.html");
                progReq.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                progReq.Headers.Add("X-Requested-With", "XMLHttpRequest");
                var progResp = await (await client.SendAsync(progReq)).Content.ReadAsStringAsync();
                lastProg = progResp;
                result.Logs.Add($"[신도다운] progress {attempt + 1}({progResp.Length}자): {progResp[..Math.Min(200, progResp.Length)]}");

                // 준비 완료 시그널: ReadyToDownload / UserCgiName:doc/... / waitend / 빈 응답(작업 종료)
                if (progResp.Length == 0
                    || progResp.Contains("ReadyToDownload")
                    || progResp.Contains("\"UserCgiName\"")
                    || progResp.Contains("\"waitend\":\"true\"")
                    || progResp.Contains("\"RedirectUrl\":\"doc"))
                {
                    result.Logs.Add("[신도다운] 준비 완료");
                    ready = true;
                    break;
                }

                var intervalMatch = Regex.Match(progResp, @"""Interval""\s*:\s*""(\d+)""");
                var interval = intervalMatch.Success ? int.Parse(intervalMatch.Groups[1].Value) : 1500;
                await Task.Delay(Math.Min(interval, 2500));
            }

            // 폴링이 끝나지 않아도 다운로드 GET을 시도 (폴링이 잘못된 엔드포인트일 수 있음)
            if (!ready)
                result.Logs.Add("[신도다운][WARN] 폴링 미완료, PDF GET 직접 시도");

            // Step 4: 실제 PDF GET (PDF가 아니면 대기 후 최대 5회 재시도)
            var pdfUrl = $"{baseUrl}/wcd/doc/{Uri.EscapeDataString(file.Name)}.pdf" +
                $"?func=PSL_F_UOU_DLD&h_token={token}" +
                $"&cginame1={Uri.EscapeDataString($"doc/{file.Name}.pdf")}" +
                $"&cginame2={Uri.EscapeDataString($"doc/{file.Name}.pdf")}" +
                $"&H_BAK=0&H_TAB=&H_DLV=";

            result.Logs.Add($"[신도다운] PDF GET: {pdfUrl}");

            byte[]? bytes = null;
            for (var attempt = 0; attempt < 6; attempt++)
            {
                var pdfReq = new HttpRequestMessage(HttpMethod.Get, pdfUrl);
                pdfReq.Headers.Add("Referer", $"{baseUrl}/wcd/spa_contents_frame.tmpl.html");
                pdfReq.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                var pdfResp = await client.SendAsync(pdfReq);

                if (!pdfResp.IsSuccessStatusCode)
                {
                    result.Logs.Add($"[신도다운] GET {attempt + 1}: HTTP {(int)pdfResp.StatusCode}");
                    ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");
                    return DriverResult<byte[]>.Fail($"HTTP {(int)pdfResp.StatusCode}", result.Logs);
                }

                var ct = pdfResp.Content.Headers.ContentType?.MediaType ?? "";
                var data = await pdfResp.Content.ReadAsByteArrayAsync();
                var isPdf = data.Length >= 4 && data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46; // %PDF
                result.Logs.Add($"[신도다운] GET {attempt + 1}: CT={ct}, {data.Length}bytes, PDF헤더={isPdf}");

                if (isPdf || ct == "application/octet-stream")
                {
                    bytes = data;
                    break;
                }

                await Task.Delay(1500);
            }

            // 쿠키 복원 (F_UOU로)
            ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");

            if (bytes == null)
                return DriverResult<byte[]>.Fail("PDF 응답 수신 실패 (서버가 PDF 반환 안함)", result.Logs);

            result.Logs.Add($"[신도다운] 완료: {bytes.Length} bytes");
            result.Success = true;
            result.Data = bytes;
            result.Message = $"{bytes.Length} bytes";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도다운][ERROR] {ex.Message}");
            return DriverResult<byte[]>.Fail($"다운로드 오류: {ex.Message}", result.Logs);
        }
    }
}
