using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Drivers.Sindoh;

/// <summary>
/// 신도리코 기본 드라이버 — 현재 대부분의 신도 모델(D450 등)에 적용되는 플로우.
/// JSON REST API (/wcd/api/) 기반 박스 CRUD.
///
/// 세션: ulogin.cgi → ID 쿠키 → AppReqGetUserBoxList에서 토큰 추출.
/// 박스 생성: AppReqSetCustomMessage/_005_001_ULU001 (func=PSL_F_ULUUser_CRE).
///
/// D420처럼 플로우가 다른 모델은 별도 클래스로 분리하고 DriverRegistry에 등록.
/// </summary>
public sealed class SindohDefaultDriver : SindohDriverBase
{
    // ──────────────────────────────────────────────
    // 세션 캐시
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

    public override void DisposeSessions()
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
    // 세션/토큰 초기화
    // ──────────────────────────────────────────────

    private static async Task<(HttpClient? client, string? token, string baseUrl, List<string> logs)>
        InitSessionAsync(MfpDevice device)
    {
        var logs = new List<string>();
        var baseUrl = string.IsNullOrEmpty(device.BaseUrl) ? $"http://{device.Ip}" : device.BaseUrl;

        if (_sessions.TryGetValue(device.Ip, out var cached))
        {
            cached.LastUsed = DateTime.UtcNow;
            return (cached.Client, cached.Token, cached.BaseUrl, logs);
        }

        var (client, cookies) = CreateClient();
        var uri = new Uri(baseUrl);

        try
        {
            logs.Add($"[신도] 세션 초기화 ({baseUrl})");

            // Step 1
            var step1 = await GetAsync(client, $"{baseUrl}/spa_login.html") ;
            if (!step1.IsSuccessStatusCode)
            {
                logs.Add("[신도][FAIL] Step1 spa_login.html 응답 실패");
                logs.Add(step1.Dump());
            }

            // Step 2: ulogin
            var loginPayload = "func=PSL_LP0_TOP&AuthType=None&TrackType=&ExtSvType=0&PswcForm=&Mode=Public" +
                "&publicuser=&username=&password=&AuthorityType=&R_ADM=&ExtServ=0&ViewMode=&BrowserMode=&Lang=" +
                "&trackname=&trackpassword=";
            var loginEx = await PostFormAsync(client, $"{baseUrl}/wcd/ulogin.cgi", loginPayload, $"{baseUrl}/wcd/spa_login.html", logs, "*/*");

            var allCookies = cookies.GetAllCookies();
            var idCookie = allCookies.FirstOrDefault(c => c.Name == "ID");
            if (idCookie == null || string.IsNullOrEmpty(idCookie.Value))
            {
                logs.Add("[신도][FAIL] 로그인 실패 — ID 쿠키 없음");
                logs.Add(loginEx.Dump());
                client.Dispose();
                return (null, null, baseUrl, logs);
            }

            // Step 3
            cookies.Add(uri, new Cookie("menuType", "Public"));
            cookies.Add(uri, new Cookie("usr", "F_ULU"));
            cookies.Add(uri, new Cookie("box_dsp", "Setting"));
            cookies.Add(uri, new Cookie("webUI", "new"));

            // Step 4
            var step4 = await GetAsync(client, $"{baseUrl}/wcd/spa_main.html");
            if (!step4.IsSuccessStatusCode)
            {
                logs.Add("[신도][WARN] spa_main.html 응답 비정상");
                logs.Add(step4.Dump());
            }

            // Step 5 — 토큰 추출
            var tokenEx = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "1" } }
                    },
                    Token = ""
                }, $"{baseUrl}/wcd/spa_main.html", logs);

            if (!tokenEx.IsSuccessStatusCode)
            {
                logs.Add("[신도][FAIL] 토큰 요청 HTTP 실패");
                logs.Add(tokenEx.Dump());
                client.Dispose();
                return (null, null, baseUrl, logs);
            }

            var tokenMatch = Regex.Match(tokenEx.Body, @"""Token""\s*:\s*""([^""]+)""");
            string? token = tokenMatch.Success ? tokenMatch.Groups[1].Value : null;

            if (token == null)
            {
                logs.Add("[신도][WARN] 토큰 미추출 — 전체 덤프:");
                logs.Add(tokenEx.Dump());
                token = "";
            }

            device.BaseUrl = baseUrl;
            _sessions[device.Ip] = new SindohSession { Client = client, Token = token ?? "", BaseUrl = baseUrl, Cookies = cookies };

            return (client, token, baseUrl, logs);
        }
        catch (Exception ex)
        {
            logs.Add($"[신도][ERROR] 세션: {ex.Message}");
            logs.Add($"[STACK] {ex.StackTrace}");
            client.Dispose();
            return (null, null, baseUrl, logs);
        }
    }

    // ──────────────────────────────────────────────
    // ConnectAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> ConnectAsync(MfpDevice device)
    {
        var result = new DriverResult();
        try
        {
            var (client, token, _, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (client == null) return DriverResult.Fail("연결 실패", result.Logs);
            device.Status = ConnectionStatus.Connected;
            result.Success = true;
            result.Message = "연결 성공";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도][ERROR] {ex.Message}");
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
            var (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null) return DriverResult<List<ScanBox>>.Fail("세션 실패", result.Logs);

            var ex = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                    },
                    Token = token ?? ""
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!ex.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도조회][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(ex.Dump());
                return DriverResult<List<ScanBox>>.Fail($"조회 실패 HTTP {ex.StatusCode}", result.Logs);
            }

            var boxes = new List<ScanBox>();
            foreach (Match m in Regex.Matches(ex.Body, @"""BoxID""\s*:\s*""(\d+)"".*?""Name""\s*:\s*""([^""]+)""", RegexOptions.Singleline))
            {
                boxes.Add(new ScanBox { Name = m.Groups[2].Value, SlotIndex = int.Parse(m.Groups[1].Value), MfpDeviceId = device.Id });
            }

            result.Logs.Add($"[신도] 박스 {boxes.Count}개 조회");
            result.Success = true;
            result.Data = boxes;
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도][ERROR] {ex.Message}");
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
            result.Logs.Add($"[신도추가] 박스 생성: {box.Name}");

            var (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            var pw = box.Password ?? "";
            var usePass = !string.IsNullOrEmpty(pw) ? "UsePass" : "NoPass";

            var ex = await PostJsonAsync(client,
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
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!ex.IsSuccessStatusCode || (!ex.Body.Contains("\"Ack\"") && !ex.Body.Contains("\"Ok_1\"")))
            {
                result.Logs.Add("[신도추가][FAIL] 박스 생성 실패 — 전체 덤프:");
                result.Logs.Add(ex.Dump());
                return DriverResult.Fail("박스 생성 실패", result.Logs);
            }

            result.Logs.Add("[신도추가] 생성 완료");
            result.Success = true;
            result.Message = "스캔함 추가 완료";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도추가][ERROR] {ex.Message}");
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
            var searchName = oldName ?? box.Name;
            result.Logs.Add($"[신도수정] 박스 수정: {searchName} → {box.Name}");

            var (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            var listEx = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                    },
                    Token = token ?? ""
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도수정][FAIL] 박스 목록 조회 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail("박스 목록 조회 실패", result.Logs);
            }

            var boxNum = FindBoxNumber(listEx.Body, searchName);
            if (boxNum == null)
            {
                result.Logs.Add($"[신도수정][FAIL] 박스 '{searchName}' 찾을 수 없음 — 목록 응답 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail($"박스 '{searchName}'을 찾을 수 없습니다.", result.Logs);
            }
            result.Logs.Add($"[신도수정] 대상 박스 번호: {boxNum}");

            var pw = box.Password ?? "";
            var changePw = !string.IsNullOrEmpty(pw);

            var ex = await PostJsonAsync(client,
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
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!ex.IsSuccessStatusCode || (!ex.Body.Contains("\"Ack\"") && !ex.Body.Contains("\"Ok_1\"")))
            {
                result.Logs.Add("[신도수정][FAIL] 수정 실패 — 전체 덤프:");
                result.Logs.Add(ex.Dump());
                return DriverResult.Fail("수정 실패", result.Logs);
            }

            result.Logs.Add("[신도수정] 수정 완료");
            result.Success = true;
            result.Message = "수정 완료";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도수정][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"수정 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // DeleteScanBoxAsync (3단계: 토큰 갱신 → 인증 → 삭제)
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        try
        {
            result.Logs.Add($"[신도삭제] 박스 삭제: {box.Name}");

            var (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null) return DriverResult.Fail("세션 실패", result.Logs);

            var listEx = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                    },
                    Token = token ?? ""
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도삭제][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail("박스 목록 조회 실패", result.Logs);
            }

            var boxNum = FindBoxNumber(listEx.Body, box.Name);
            if (boxNum == null)
            {
                result.Logs.Add($"[신도삭제][FAIL] 박스 '{box.Name}' 목록에서 찾을 수 없음 — 응답 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail($"박스 '{box.Name}'을 찾을 수 없습니다.", result.Logs);
            }
            result.Logs.Add($"[신도삭제] 대상 박스 ID: {boxNum}");

            var pw = box.Password ?? "";
            var uri = new Uri(baseUrl);

            // Step 1: 토큰 갱신을 위해 usr 쿠키 교체 (세션 캐시의 쿠키 사용)
            if (!_sessions.TryGetValue(device.Ip, out var session))
                return DriverResult.Fail("세션 캐시 없음", result.Logs);

            session.Cookies.Add(uri, new Cookie("usr", "F_ULUUserBoxLogin"));

            var tokenEx = await GetAsync(client, $"{baseUrl}/wcd/token.json?_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", result.Logs);
            if (!tokenEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도삭제][FAIL] token.json 실패");
                result.Logs.Add(tokenEx.Dump());
                return DriverResult.Fail("토큰 갱신 실패", result.Logs);
            }
            var tokenMatch = Regex.Match(tokenEx.Body, @"""Token""\s*:\s*""([^""]+)""");
            if (tokenMatch.Success) token = tokenMatch.Groups[1].Value;

            // Step 2: 비밀번호 인증 (form-urlencoded)
            var authPayload = $"func=PSL_F_ULUUser_BOX&h_token={token}&H_TAB=&H_BID={boxNum}&H_DSP=Delete&H_IPA=On&T_BID={boxNum}&H_BAT=Public&P_BPA={Uri.EscapeDataString(pw)}";
            var authEx = await PostFormAsync(client, $"{baseUrl}/wcd/api/AppReqSetCustomMessage/_005_001_ULU004", authPayload, $"{baseUrl}/wcd/spa_main.html", result.Logs);
            if (!authEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도삭제][FAIL] 비밀번호 인증 HTTP 실패");
                result.Logs.Add(authEx.Dump());
                return DriverResult.Fail("비밀번호 인증 실패", result.Logs);
            }

            // Step 3: 실제 삭제
            var ex = await PostJsonAsync(client,
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
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!ex.IsSuccessStatusCode || (!ex.Body.Contains("\"Ack\"") && !ex.Body.Contains("\"Ok_1\"")))
            {
                result.Logs.Add("[신도삭제][FAIL] 삭제 실패 — 전체 덤프(인증 포함):");
                result.Logs.Add(authEx.Dump());
                result.Logs.Add(ex.Dump());
                return DriverResult.Fail("삭제 실패", result.Logs);
            }

            result.Logs.Add("[신도삭제] 삭제 완료");
            result.Success = true;
            result.Message = "삭제 완료";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도삭제][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"삭제 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // 유틸: 박스 이름 → BoxID
    // ──────────────────────────────────────────────

    private static string? FindBoxNumber(string json, string boxName)
    {
        var blocks = Regex.Matches(json, @"""BoxID""\s*:\s*""(\d+)"".*?""Name""\s*:\s*""([^""]+)""", RegexOptions.Singleline);
        foreach (Match m in blocks)
        {
            if (m.Groups[2].Value == boxName)
                return m.Groups[1].Value;
        }
        return null;
    }

    // ──────────────────────────────────────────────
    // GetBoxFilesAsync (3단계: 쿠키 전환 → 박스 진입 → waitmsg 폴링)
    // ──────────────────────────────────────────────

    public override async Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult<List<BoxFile>> { Logs = [] };
        try
        {
            result.Logs.Add($"[신도파일] 박스 '{box.Name}' 파일 목록 조회");

            var (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null || token == null)
                return DriverResult<List<BoxFile>>.Fail("세션 실패", result.Logs);

            if (!_sessions.TryGetValue(device.Ip, out var session))
                return DriverResult<List<BoxFile>>.Fail("세션 캐시 없음", result.Logs);

            var listEx = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                    },
                    Token = token
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도파일][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult<List<BoxFile>>.Fail("박스 목록 조회 실패", result.Logs);
            }

            var boxNum = FindBoxNumber(listEx.Body, box.Name);
            if (boxNum == null)
            {
                result.Logs.Add("[신도파일][FAIL] 박스 미존재 — 목록 응답 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult<List<BoxFile>>.Fail($"박스 '{box.Name}' 찾을 수 없음", result.Logs);
            }

            result.Logs.Add($"[신도파일] 박스 번호: {boxNum}");

            var uri = new Uri(baseUrl);
            var wcdUri = new Uri($"{baseUrl}/wcd/");
            ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");
            var pw = box.Password ?? "";
            var enterEx = await PostJsonAsync(client,
                $"{baseUrl}/wcd/api/AppReqSetCustomMessage/_105_000_ULU000",
                new {
                    func = "PSL_F_UOUUser_BOX",
                    H_BID = boxNum, T_BID = boxNum, P_BPA = pw,
                    h_token = token, H_PID = "-1", H_IPA = "On",
                    H_BAT = "Public", _ = "",
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!enterEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도파일][FAIL] 박스 진입 실패");
                result.Logs.Add(enterEx.Dump());
                return DriverResult<List<BoxFile>>.Fail("박스 진입 실패", result.Logs);
            }

            // Step 3-A
            var firstEx = await PostFormAsync(client, $"{baseUrl}/wcd/box_detail.json",
                "waitend=true&TaskNo=0&_=", $"{baseUrl}/wcd/spa_main.html", result.Logs);
            if (!firstEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도파일][FAIL] box_detail.json 첫 요청 실패");
                result.Logs.Add(firstEx.Dump());
                return DriverResult<List<BoxFile>>.Fail("데이터 준비 실패", result.Logs);
            }
            var firstResp = firstEx.Body;

            var taskNo = "0";
            var taskMatch = Regex.Match(firstResp, @"""ParamValue""\s*:\s*""([^""]+)""");
            if (taskMatch.Success) taskNo = taskMatch.Groups[1].Value;

            // Step 3-B: waitmsg polling
            HttpExchange? lastWaitEx = null;
            var ready = false;
            var pollAttempts = 0;
            for (var attempt = 0; attempt < 30; attempt++)
            {
                pollAttempts = attempt + 1;
                lastWaitEx = await PostFormAsync(client, $"{baseUrl}/wcd/waitmsg",
                    $"TaskNo={taskNo}&_=", $"{baseUrl}/wcd/spa_main.html");

                if (lastWaitEx.Body.Contains("\"waitend\":\"true\""))
                {
                    ready = true;
                    break;
                }

                var intervalMatch = Regex.Match(lastWaitEx.Body, @"""Interval""\s*:\s*""(\d+)""");
                var interval = intervalMatch.Success ? int.Parse(intervalMatch.Groups[1].Value) : 200;
                await Task.Delay(Math.Min(interval, 500));
            }

            if (!ready)
            {
                result.Logs.Add($"[신도파일][FAIL] waitmsg 타임아웃 ({pollAttempts}회 시도) — 마지막 응답 덤프:");
                if (lastWaitEx != null) result.Logs.Add(lastWaitEx.Dump());
                return DriverResult<List<BoxFile>>.Fail("데이터 준비 타임아웃", result.Logs);
            }

            // Step 3-C
            var finalEx = await PostFormAsync(client, $"{baseUrl}/wcd/box_detail.json",
                $"TaskNo={taskNo}&waitend=true", $"{baseUrl}/wcd/spa_main.html");
            if (!finalEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도파일][FAIL] box_detail.json 최종 요청 실패");
                result.Logs.Add(finalEx.Dump());
                return DriverResult<List<BoxFile>>.Fail("파일 목록 수신 실패", result.Logs);
            }
            var detailResp = finalEx.Body;

            var actualBoxId = ExtractBoxId(detailResp);
            if (actualBoxId != null && actualBoxId != boxNum)
            {
                result.Logs.Add($"[신도파일][WARN] 박스 불일치 — 응답={actualBoxId}, 요청={boxNum}. 세션 무효화. 상세 덤프:");
                result.Logs.Add(finalEx.Dump());
                InvalidateSession(device.Ip);
                return DriverResult<List<BoxFile>>.Fail($"박스 불일치 (응답={actualBoxId}, 요청={boxNum})", result.Logs);
            }

            var files = ParseSindohBoxFiles(detailResp);
            result.Success = true;
            result.Data = files;
            result.Message = $"{files.Count}개 파일";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도파일][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"파일 목록 오류: {ex.Message}";
            return result;
        }
    }

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
        catch { }

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
    // DownloadFileAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file)
    {
        var result = new DriverResult<byte[]> { Logs = [] };
        try
        {
            result.Logs.Add($"[신도다운] 파일 다운로드: {file.Name} (박스={box.Name})");

            var (client, token, baseUrl, loginLogs) = await InitSessionAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null || token == null)
                return DriverResult<byte[]>.Fail("세션 실패", result.Logs);

            if (!_sessions.TryGetValue(device.Ip, out var session))
                return DriverResult<byte[]>.Fail("세션 캐시 없음", result.Logs);

            var listEx = await PostJsonAsync(client, $"{baseUrl}/wcd/api/AppReqGetUserBoxList",
                new {
                    BoxListCondition = new {
                        SearchKey = "None", WellUse = "false",
                        BoxAttribute = new { Category = "Functional", Type = "User", Attribute = "AllAttribute" },
                        ObtainCondition = new { Type = "OffsetList", OffsetRange = new { Start = "1", Length = "50" } }
                    },
                    Token = token
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도다운][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult<byte[]>.Fail("박스 목록 조회 실패", result.Logs);
            }

            var boxNum = FindBoxNumber(listEx.Body, box.Name);
            if (boxNum == null)
            {
                result.Logs.Add("[신도다운][FAIL] 박스 찾을 수 없음 — 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult<byte[]>.Fail($"박스 '{box.Name}' 찾을 수 없음", result.Logs);
            }

            var uri = new Uri(baseUrl);
            var wcdUri = new Uri($"{baseUrl}/wcd/");
            ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU_FileDownload");

            var pw = box.Password ?? "";
            var prepEx = await PostJsonAsync(client,
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
                }, $"{baseUrl}/wcd/spa_main.html", result.Logs);

            if (!prepEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도다운][FAIL] 다운로드 준비 요청 실패");
                result.Logs.Add(prepEx.Dump());
                ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");
                return DriverResult<byte[]>.Fail("다운로드 준비 실패", result.Logs);
            }

            var initIntervalMatch = Regex.Match(prepEx.Body, @"""Interval""\s*:\s*""(\d+)""");
            var initInterval = initIntervalMatch.Success ? int.Parse(initIntervalMatch.Groups[1].Value) : 2500;
            await Task.Delay(Math.Min(initInterval, 2500));

            HttpExchange? lastProgEx = null;
            var ready = false;
            var progressAttempts = 0;
            for (var attempt = 0; attempt < 30; attempt++)
            {
                progressAttempts = attempt + 1;
                lastProgEx = await PostFormAsync(client, $"{baseUrl}/wcd/progress",
                    "_=", $"{baseUrl}/wcd/spa_main.html");

                var progResp = lastProgEx.Body;
                if (progResp.Length == 0
                    || progResp.Contains("ReadyToDownload")
                    || progResp.Contains("\"UserCgiName\"")
                    || progResp.Contains("\"waitend\":\"true\"")
                    || progResp.Contains("\"RedirectUrl\":\"doc"))
                {
                    ready = true;
                    break;
                }

                var intervalMatch = Regex.Match(progResp, @"""Interval""\s*:\s*""(\d+)""");
                var interval = intervalMatch.Success ? int.Parse(intervalMatch.Groups[1].Value) : 1500;
                await Task.Delay(Math.Min(interval, 2500));
            }

            if (!ready)
            {
                result.Logs.Add($"[신도다운][WARN] 폴링 미완료({progressAttempts}회), 마지막 progress 응답:");
                if (lastProgEx != null) result.Logs.Add(lastProgEx.Dump());
                result.Logs.Add("[신도다운][WARN] PDF GET 직접 시도");
            }

            var pdfUrl = $"{baseUrl}/wcd/doc/{Uri.EscapeDataString(file.Name)}.pdf" +
                $"?func=PSL_F_UOU_DLD&h_token={token}" +
                $"&cginame1={Uri.EscapeDataString($"doc/{file.Name}.pdf")}" +
                $"&cginame2={Uri.EscapeDataString($"doc/{file.Name}.pdf")}" +
                $"&H_BAK=0&H_TAB=&H_DLV=";

            result.Logs.Add($"[HTTP→] GET ({pdfUrl})");

            byte[]? bytes = null;
            HttpExchange? lastPdfEx = null;
            var getAttempts = 0;
            for (var attempt = 0; attempt < 6; attempt++)
            {
                getAttempts = attempt + 1;
                var pdfReq = new HttpRequestMessage(HttpMethod.Get, pdfUrl);
                pdfReq.Headers.Add("Referer", $"{baseUrl}/wcd/spa_contents_frame.tmpl.html");
                pdfReq.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                var (pdfEx, data) = await HttpDiagnostics.SendBytesAsync(client, pdfReq);
                lastPdfEx = pdfEx;

                if (!pdfEx.IsSuccessStatusCode)
                {
                    result.Logs.Add($"[신도다운][FAIL] GET 실패 HTTP {pdfEx.StatusCode} — 덤프:");
                    result.Logs.Add(pdfEx.Dump());
                    ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");
                    return DriverResult<byte[]>.Fail($"HTTP {pdfEx.StatusCode}", result.Logs);
                }

                var isPdf = data.Length >= 4 && data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46; // %PDF
                var ct = pdfEx.ResponseHeaders.Contains("Content-Type: application/octet-stream");

                if (isPdf || ct)
                {
                    bytes = data;
                    break;
                }

                await Task.Delay(1500);
            }

            ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");

            if (bytes == null)
            {
                result.Logs.Add($"[신도다운][FAIL] PDF 수신 실패 ({getAttempts}회 시도) — 마지막 GET 응답:");
                if (lastPdfEx != null) result.Logs.Add(lastPdfEx.Dump());
                return DriverResult<byte[]>.Fail("PDF 응답 수신 실패 (서버가 PDF 반환 안함)", result.Logs);
            }

            result.Logs.Add($"[신도다운] 완료: {bytes.Length} bytes");
            result.Success = true;
            result.Data = bytes;
            result.Message = $"{bytes.Length} bytes";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도다운][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            return DriverResult<byte[]>.Fail($"다운로드 오류: {ex.Message}", result.Logs);
        }
    }
}
