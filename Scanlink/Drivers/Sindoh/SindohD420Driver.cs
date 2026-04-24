using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Models;

namespace Scanlink.Drivers.Sindoh;

/// <summary>
/// 신도 D420 전용 드라이버 — 레거시 /wcd/user.cgi + HTML 인터페이스.
///
/// D450(SindohDefaultDriver)와의 차이:
///   - REST JSON API (/wcd/api/AppReqSetCustomMessage/...) 미사용
///   - 모든 박스 CRUD가 /wcd/user.cgi 에 form-urlencoded POST
///   - 박스 목록은 GET /wcd/box_list.xml (실제로는 HTML 반환)
///   - 토큰/박스 ID/이름 모두 HTML hidden input에서 파싱
///
/// 파일 목록/다운로드는 추후 구현(현재는 미구현 반환).
/// </summary>
public sealed class SindohD420Driver : SindohDriverBase
{
    // ──────────────────────────────────────────────
    // 세션 캐시 (기기 IP 단위)
    // ──────────────────────────────────────────────

    private sealed class D420Session
    {
        public required HttpClient Client { get; init; }
        public required string BaseUrl { get; init; }
        public required CookieContainer Cookies { get; init; }
        public string Token { get; set; } = "";
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, D420Session> _sessions = new();

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
    // 세션 초기화 (ulogin.cgi → 쿠키 → box_list.xml)
    // ──────────────────────────────────────────────

    private static async Task<(D420Session? session, List<string> logs)> InitSessionAsync(MfpDevice device)
    {
        var logs = new List<string>();
        var baseUrl = string.IsNullOrEmpty(device.BaseUrl) ? $"http://{device.Ip}" : device.BaseUrl;

        if (_sessions.TryGetValue(device.Ip, out var cached))
        {
            cached.LastUsed = DateTime.UtcNow;
            return (cached, logs);
        }

        var (client, cookies) = CreateClient();
        var uri = new Uri(baseUrl);

        try
        {
            logs.Add($"[신도D420] 세션 초기화 ({baseUrl})");

            // Step 1: 루트 페이지 GET(세션 쿠키 준비)
            var step1 = await GetAsync(client, $"{baseUrl}/", logs);
            if (!step1.IsSuccessStatusCode)
            {
                logs.Add("[신도D420][WARN] 루트 페이지 응답 비정상");
                logs.Add(step1.Dump());
            }

            // Step 2: Public 로그인 — D420은 spa_login.html이 없고 ulogin.cgi만 존재
            var loginPayload = "func=PSL_LP0_TOP&AuthType=None&TrackType=&ExtSvType=0&PswcForm=&Mode=Public" +
                "&publicuser=&username=&password=&AuthorityType=&R_ADM=&ExtServ=0&ViewMode=&BrowserMode=&Lang=" +
                "&trackname=&trackpassword=";
            var loginEx = await PostFormAsync(client, $"{baseUrl}/wcd/ulogin.cgi", loginPayload, $"{baseUrl}/", logs, "*/*");

            // 쿠키 세팅(ID 쿠키가 이미 있으면 덮어씀)
            cookies.Add(uri, new Cookie("menuType", "Public"));
            cookies.Add(uri, new Cookie("usr", "F_ULU"));
            cookies.Add(uri, new Cookie("box_dsp", "Setting"));

            // Step 3: 박스 목록 페이지 → 토큰 추출
            var listEx = await GetAsync(client, $"{baseUrl}/wcd/box_list.xml", logs);
            if (!listEx.IsSuccessStatusCode)
            {
                logs.Add("[신도D420][FAIL] box_list.xml HTTP 실패 — 로그인/박스 응답 덤프:");
                logs.Add(loginEx.Dump());
                logs.Add(listEx.Dump());
                client.Dispose();
                return (null, logs);
            }

            var token = ExtractToken(listEx.Body);
            if (token == null)
            {
                logs.Add("[신도D420][FAIL] h_token 추출 실패 — 로그인/박스 응답 덤프:");
                logs.Add(loginEx.Dump());
                logs.Add(listEx.Dump());
                client.Dispose();
                return (null, logs);
            }

            device.BaseUrl = baseUrl;
            var session = new D420Session { Client = client, BaseUrl = baseUrl, Cookies = cookies, Token = token };
            _sessions[device.Ip] = session;
            logs.Add($"[신도D420] 세션 확보 (token 앞15자: {token[..Math.Min(15, token.Length)]}...)");
            return (session, logs);
        }
        catch (Exception ex)
        {
            logs.Add($"[신도D420][ERROR] 세션: {ex.Message}");
            logs.Add($"[STACK] {ex.StackTrace}");
            client.Dispose();
            return (null, logs);
        }
    }

    // ──────────────────────────────────────────────
    // HTML 파싱 유틸
    // ──────────────────────────────────────────────

    private static string? ExtractToken(string html)
    {
        var m = Regex.Match(html, @"name=[""']h_token[""']\s+value=[""']([^""']+)[""']");
        return m.Success ? m.Groups[1].Value : null;
    }

    /// <summary>
    /// box_list.xml HTML에서 박스 번호/이름 쌍을 추출.
    /// 각 박스는 id="{n}_BID" / id="{n}_BNM" hidden input 쌍으로 렌더됨.
    /// </summary>
    private static List<(string boxId, string name)> ParseBoxes(string html)
    {
        var bidMap = new Dictionary<string, string>();
        var bnmMap = new Dictionary<string, string>();

        foreach (Match m in Regex.Matches(html, @"id=[""'](\d+)_BID[""']\s+value=[""'](\d+)[""']"))
            bidMap[m.Groups[1].Value] = m.Groups[2].Value;

        foreach (Match m in Regex.Matches(html, @"id=[""'](\d+)_BNM[""']\s+value=[""']([^""']*)[""']"))
            bnmMap[m.Groups[1].Value] = m.Groups[2].Value;

        var result = new List<(string, string)>();
        foreach (var kv in bidMap)
        {
            if (bnmMap.TryGetValue(kv.Key, out var name))
                result.Add((kv.Value, name));
        }
        return result;
    }

    private static async Task<(List<(string boxId, string name)> boxes, HttpExchange ex)>
        FetchBoxesAsync(D420Session session, List<string>? logs = null)
    {
        var ex = await GetAsync(session.Client, $"{session.BaseUrl}/wcd/box_list.xml", logs);
        // 토큰이 바뀌었을 수 있으니 갱신
        var newToken = ExtractToken(ex.Body);
        if (newToken != null) session.Token = newToken;
        return (ParseBoxes(ex.Body), ex);
    }

    // ──────────────────────────────────────────────
    // ConnectAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> ConnectAsync(MfpDevice device)
    {
        var result = new DriverResult();
        try
        {
            var (session, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (session == null) return DriverResult.Fail("연결 실패", result.Logs);

            device.Status = ConnectionStatus.Connected;
            result.Success = true;
            result.Message = "연결 성공";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도D420][ERROR] {ex.Message}");
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
            var (session, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (session == null) return DriverResult<List<ScanBox>>.Fail("세션 실패", result.Logs);

            var (boxes, ex) = await FetchBoxesAsync(session, result.Logs);
            if (!ex.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도D420조회][FAIL] box_list.xml HTTP 실패");
                result.Logs.Add(ex.Dump());
                return DriverResult<List<ScanBox>>.Fail($"조회 HTTP {ex.StatusCode}", result.Logs);
            }

            var scanBoxes = boxes.Select(b => new ScanBox
            {
                Name = b.name,
                SlotIndex = int.Parse(b.boxId),
                MfpDeviceId = device.Id,
            }).ToList();

            result.Logs.Add($"[신도D420] 박스 {scanBoxes.Count}개 조회");
            result.Success = true;
            result.Data = scanBoxes;
            result.Message = $"{scanBoxes.Count}개 조회";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도D420][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"조회 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // AddScanBoxAsync — user.cgi / func=PSL_F_ULUUser_CRE
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        try
        {
            result.Logs.Add($"[신도D420추가] 박스 생성: {box.Name}");

            var (session, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (session == null) return DriverResult.Fail("세션 실패", result.Logs);

            var pw = box.Password ?? "";
            var usePass = !string.IsNullOrEmpty(pw) ? "UsePass" : "NoPass";

            var payload =
                $"func=PSL_F_ULUUser_CRE" +
                $"&h_token={Uri.EscapeDataString(session.Token)}" +
                $"&H_TAB=" +
                $"&R_NUM=Space" +
                $"&T_NAM={Uri.EscapeDataString(box.Name)}" +
                $"&C_USE={usePass}" +
                $"&P_PAS={Uri.EscapeDataString(pw)}" +
                $"&P_PAS2={Uri.EscapeDataString(pw)}" +
                $"&S_SER=Abc" +
                $"&R_SAP=None" +
                $"&S_GFC=On" +
                $"&S_SEC=Off";

            var ex = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/user.cgi",
                payload, $"{session.BaseUrl}/wcd/box_create.xml", result.Logs,
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            if (!ex.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도D420추가][FAIL] user.cgi HTTP 실패");
                result.Logs.Add(ex.Dump());
                return DriverResult.Fail($"박스 생성 실패 HTTP {ex.StatusCode}", result.Logs);
            }

            // 응답은 보통 갱신된 box_list.xml(HTML) — 해당 이름이 리스트에 있으면 성공
            var afterBoxes = ParseBoxes(ex.Body);
            var created = afterBoxes.Any(b => b.name == box.Name);
            if (!created)
            {
                // fallback: 박스 목록 재조회해 검증
                var (boxes2, listEx) = await FetchBoxesAsync(session, result.Logs);
                created = boxes2.Any(b => b.name == box.Name);
                if (!created)
                {
                    result.Logs.Add("[신도D420추가][FAIL] 생성 검증 실패 — 응답/목록 덤프:");
                    result.Logs.Add(ex.Dump());
                    result.Logs.Add(listEx.Dump());
                    return DriverResult.Fail("박스 생성 실패 (목록에 없음)", result.Logs);
                }
            }

            result.Logs.Add("[신도D420추가] 생성 완료");
            result.Success = true;
            result.Message = "스캔함 추가 완료";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도D420추가][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"추가 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // UpdateScanBoxAsync — user.cgi / func=PSL_F_ULU_SET
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null)
    {
        var result = new DriverResult();
        try
        {
            var searchName = oldName ?? box.Name;
            result.Logs.Add($"[신도D420수정] 박스 수정: {searchName} → {box.Name}");

            var (session, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (session == null) return DriverResult.Fail("세션 실패", result.Logs);

            var (boxes, listEx) = await FetchBoxesAsync(session, result.Logs);
            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도D420수정][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail("박스 목록 조회 실패", result.Logs);
            }

            var target = boxes.FirstOrDefault(b => b.name == searchName);
            if (target == default)
            {
                result.Logs.Add($"[신도D420수정][FAIL] 박스 '{searchName}' 찾을 수 없음 — 목록 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail($"박스 '{searchName}'을 찾을 수 없습니다.", result.Logs);
            }
            result.Logs.Add($"[신도D420수정] 대상 박스 ID: {target.boxId}");

            var newPw = box.Password ?? "";
            var changePw = !string.IsNullOrEmpty(newPw);

            var payload =
                $"H_TAB=" +
                $"&func=PSL_F_ULU_SET" +
                $"&h_token={Uri.EscapeDataString(session.Token)}" +
                $"&H_BOX={target.boxId}" +
                $"&H_USR=" +
                $"&H_BPA=" +
                $"&H_BTY=User" +
                $"&H_BAT=Public" +
                $"&H_XTP=" +
                $"&H_DSP=Setting" +
                $"&T_NAM={Uri.EscapeDataString(box.Name)}" +
                $"&S_SER=Abc" +
                $"&R_SAP=None" +
                (changePw
                    ? $"&C_PAC=on&P_CPA={Uri.EscapeDataString(oldPassword ?? "")}&P_NPA={Uri.EscapeDataString(newPw)}&P_NPA2={Uri.EscapeDataString(newPw)}"
                    : $"&C_PAC=&P_CPA=&P_NPA=&P_NPA2=");

            var ex = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/user.cgi",
                payload, $"{session.BaseUrl}/wcd/box_list.xml", result.Logs,
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            if (!ex.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도D420수정][FAIL] user.cgi HTTP 실패");
                result.Logs.Add(ex.Dump());
                return DriverResult.Fail($"수정 실패 HTTP {ex.StatusCode}", result.Logs);
            }

            // 검증: 이름이 바뀌었으면 새 이름으로 목록에 있어야 함
            var afterBoxes = ParseBoxes(ex.Body);
            var ok = afterBoxes.Any(b => b.boxId == target.boxId && b.name == box.Name);
            if (!ok)
            {
                var (boxes2, listEx2) = await FetchBoxesAsync(session, result.Logs);
                ok = boxes2.Any(b => b.boxId == target.boxId && b.name == box.Name);
                if (!ok)
                {
                    result.Logs.Add("[신도D420수정][FAIL] 수정 검증 실패 — 응답/재조회 덤프:");
                    result.Logs.Add(ex.Dump());
                    result.Logs.Add(listEx2.Dump());
                    return DriverResult.Fail("수정 실패 (검증 실패)", result.Logs);
                }
            }

            result.Logs.Add("[신도D420수정] 수정 완료");
            result.Success = true;
            result.Message = "수정 완료";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도D420수정][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"수정 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // DeleteScanBoxAsync — user.cgi / func=PSL_F_ULU_DEL
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        try
        {
            result.Logs.Add($"[신도D420삭제] 박스 삭제: {box.Name}");

            var (session, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (session == null) return DriverResult.Fail("세션 실패", result.Logs);

            var (boxes, listEx) = await FetchBoxesAsync(session, result.Logs);
            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도D420삭제][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail("박스 목록 조회 실패", result.Logs);
            }

            var target = boxes.FirstOrDefault(b => b.name == box.Name);
            if (target == default)
            {
                result.Logs.Add($"[신도D420삭제][FAIL] 박스 '{box.Name}' 찾을 수 없음 — 목록 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult.Fail($"박스 '{box.Name}'을 찾을 수 없습니다.", result.Logs);
            }
            result.Logs.Add($"[신도D420삭제] 대상 박스 ID: {target.boxId}");

            var payload =
                $"func=PSL_F_ULU_DEL" +
                $"&h_token={Uri.EscapeDataString(session.Token)}" +
                $"&H_BOX={target.boxId}" +
                $"&H_USR=" +
                $"&H_BPA={Uri.EscapeDataString(box.Password ?? "")}" +
                $"&H_NAM={Uri.EscapeDataString(box.Name)}" +
                $"&H_SAV=0" +
                $"&H_BTY=User" +
                $"&H_XTP=" +
                $"&H_DCNT=0" +
                $"&H_TAB=";

            var ex = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/user.cgi",
                payload, $"{session.BaseUrl}/wcd/box_list.xml", result.Logs,
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            if (!ex.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도D420삭제][FAIL] user.cgi HTTP 실패");
                result.Logs.Add(ex.Dump());
                return DriverResult.Fail($"삭제 실패 HTTP {ex.StatusCode}", result.Logs);
            }

            // 검증: 박스가 목록에서 사라졌는지 확인
            var afterBoxes = ParseBoxes(ex.Body);
            var stillExists = afterBoxes.Any(b => b.boxId == target.boxId && b.name == box.Name);
            if (stillExists)
            {
                var (boxes2, listEx2) = await FetchBoxesAsync(session, result.Logs);
                stillExists = boxes2.Any(b => b.boxId == target.boxId && b.name == box.Name);
                if (stillExists)
                {
                    result.Logs.Add("[신도D420삭제][FAIL] 박스가 아직 존재 — 응답/재조회 덤프:");
                    result.Logs.Add(ex.Dump());
                    result.Logs.Add(listEx2.Dump());
                    return DriverResult.Fail("삭제 실패 (박스가 목록에 남아있음)", result.Logs);
                }
            }

            result.Logs.Add("[신도D420삭제] 삭제 완료");
            result.Success = true;
            result.Message = "삭제 완료";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도D420삭제][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"삭제 오류: {ex.Message}";
            return result;
        }
    }

    // ──────────────────────────────────────────────
    // 파일 기능 — 추후 구현
    // ──────────────────────────────────────────────

    public override Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box)
    {
        return Task.FromResult(DriverResult<List<BoxFile>>.Ok(new List<BoxFile>(), "신도 D420 파일 목록 미구현"));
    }

    public override Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file)
    {
        return Task.FromResult(DriverResult<byte[]>.Fail("신도 D420 파일 다운로드 미구현"));
    }
}
