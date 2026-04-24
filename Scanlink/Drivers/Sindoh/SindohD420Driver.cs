using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Scanlink.Core;
using Scanlink.Models;

namespace Scanlink.Drivers.Sindoh;

/// <summary>
/// 신도 D420 전용 드라이버 — 레거시 /wcd/user.cgi + XML 인터페이스.
///
/// D450(SindohDefaultDriver)와의 차이:
///   - REST JSON API (/wcd/api/AppReqSetCustomMessage/...) 미사용
///   - 모든 박스 CRUD가 /wcd/user.cgi 에 form-urlencoded POST
///   - 박스 목록은 GET /wcd/box_list.xml (XML 응답, XDocument 파싱)
///   - 토큰은 응답 XML &lt;Token&gt; 또는 HTML input id="h_token" 에서 파싱
///
/// 구현 상태:
///   - 박스 목록/생성/수정/삭제: 구현
///   - 박스 내 파일 목록: 구현 (user.cgi 진입 → waitmsg 폴링 → box_detail.xml)
///   - 파일 다운로드: 구현 (NXT → DWN → progress → doc/{name}.pdf)
///     FileWatchService 가 신규 파일 감지 시 자동 호출하여 box.LocalFolder 로 저장.
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
        if (!_sessions.TryRemove(deviceIp, out var old)) return;

        // 서버측 세션도 해제해야 D420 의 웹 세션 슬롯이 즉시 반환됨.
        // 실패해도 무시 (네트워크 끊김/타임아웃 시 클라이언트만 정리).
        try
        {
            var payload = $"func=PSL_ACO_LGO&h_token={Uri.EscapeDataString(old.Token)}";
            var req = new HttpRequestMessage(HttpMethod.Post, $"{old.BaseUrl}/wcd/user.cgi")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            req.Headers.Add("Referer", $"{old.BaseUrl}/wcd/box_list.xml");
            // fire-and-forget, short timeout — 세션이 이미 죽었으면 빨리 포기
            var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(3));
            _ = old.Client.SendAsync(req, cts.Token);
        }
        catch { /* 로그아웃 실패 무시 */ }

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
    // 응답 파싱 유틸 (XML 우선, HTML fallback)
    // ──────────────────────────────────────────────

    /// <summary>
    /// h_token 추출.
    /// 우선순위:
    ///   1) XML &lt;Token&gt;...&lt;/Token&gt; — box_list.xml 정상 응답
    ///   2) HTML input의 id 또는 name 이 "h_token" 인 경우 — ulogin.cgi 응답
    /// </summary>
    private static string? ExtractToken(string content)
    {
        if (string.IsNullOrEmpty(content)) return null;

        var xml = Regex.Match(content, @"<Token>([^<]+)</Token>");
        if (xml.Success) return xml.Groups[1].Value;

        var attrFirst = Regex.Match(content, @"(?:id|name)\s*=\s*[""']h_token[""'][^>]*\svalue\s*=\s*[""']([^""']+)[""']");
        if (attrFirst.Success) return attrFirst.Groups[1].Value;

        var valueFirst = Regex.Match(content, @"value\s*=\s*[""']([^""']+)[""'][^>]*\s(?:id|name)\s*=\s*[""']h_token[""']");
        if (valueFirst.Success) return valueFirst.Groups[1].Value;

        return null;
    }

    /// <summary>
    /// box_list.xml의 &lt;BoxInfo&gt; 블록에서 박스 번호/이름 쌍 추출.
    /// XML 파싱 실패 시 빈 리스트 반환.
    /// </summary>
    private static List<(string boxId, string name)> ParseBoxes(string body)
    {
        var result = new List<(string, string)>();
        if (string.IsNullOrWhiteSpace(body)) return result;

        try
        {
            var doc = XDocument.Parse(body);
            foreach (var bi in doc.Descendants("BoxInfo"))
            {
                var id = bi.Element("BoxID")?.Value;
                var name = bi.Element("Name")?.Value;
                if (!string.IsNullOrEmpty(id))
                    result.Add((id, name ?? ""));
            }
        }
        catch (Exception)
        {
            // XML이 아닌 응답(에러 HTML 등)은 빈 리스트로 반환. 호출부에서 ex.Dump()로 확인.
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
    // GetBoxFilesAsync — 3단계 플로우
    //   1) POST user.cgi (func=PSL_F_UOUUser_BOX)  — 박스 진입, TaskNo 개시
    //   2) POST waitmsg (TaskNo=N)                 — 데이터 준비 폴링
    //   3) POST box_detail.xml (waitend=true&TaskNo=N) — 최종 파일 목록 XML 수신
    //
    // 쿠키 usr 를 F_ULU → F_UOU 로 전환 후, 완료 시 F_ULU 로 복원.
    // ──────────────────────────────────────────────

    public override async Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult<List<BoxFile>> { Logs = [] };
        try
        {
            result.Logs.Add($"[신도D420파일] 박스 '{box.Name}' 파일 목록 조회");

            var (session, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (session == null) return DriverResult<List<BoxFile>>.Fail("세션 실패", result.Logs);

            // 박스 ID 확인
            var (boxes, listEx) = await FetchBoxesAsync(session, result.Logs);
            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도D420파일][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult<List<BoxFile>>.Fail("박스 목록 조회 실패", result.Logs);
            }
            var target = boxes.FirstOrDefault(b => b.name == box.Name);
            if (target == default)
            {
                result.Logs.Add($"[신도D420파일][FAIL] 박스 '{box.Name}' 없음 — 목록 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult<List<BoxFile>>.Fail($"박스 '{box.Name}' 찾을 수 없음", result.Logs);
            }
            result.Logs.Add($"[신도D420파일] 박스 ID={target.boxId}");

            var uri = new Uri(session.BaseUrl);
            var wcdUri = new Uri($"{session.BaseUrl}/wcd/");
            ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");

            try
            {
                var pw = box.Password ?? "";

                // Step 1: 박스 진입 (PSL_F_UOUUser_BOX)
                var enterPayload =
                    $"func=PSL_F_UOUUser_BOX" +
                    $"&H_BID={target.boxId}" +
                    $"&T_BID={target.boxId}" +
                    $"&P_BPA={Uri.EscapeDataString(pw)}" +
                    $"&h_token={Uri.EscapeDataString(session.Token)}" +
                    $"&H_PID=-1" +
                    $"&H_IPA=On" +
                    $"&_=";
                var enterEx = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/user.cgi",
                    enterPayload, $"{session.BaseUrl}/wcd/box_list.xml", result.Logs);

                if (!enterEx.IsSuccessStatusCode)
                {
                    result.Logs.Add("[신도D420파일][FAIL] 박스 진입 HTTP 실패");
                    result.Logs.Add(enterEx.Dump());
                    return DriverResult<List<BoxFile>>.Fail("박스 진입 실패", result.Logs);
                }

                // 응답에서 TaskNo 추출 (<ParamValue>N</ParamValue>), 없으면 0
                var taskNo = "0";
                var tm = Regex.Match(enterEx.Body, @"<ParamValue>([^<]+)</ParamValue>");
                if (tm.Success) taskNo = tm.Groups[1].Value;

                // Step 2: waitmsg 폴링 — CgiAction 이 waitmsg 가 아니면 준비 완료
                HttpExchange? lastWait = null;
                var ready = false;
                for (var attempt = 0; attempt < 30; attempt++)
                {
                    lastWait = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/waitmsg",
                        $"TaskNo={taskNo}&_=", $"{session.BaseUrl}/wcd/box_list.xml");

                    var cgi = Regex.Match(lastWait.Body, @"<CgiAction>([^<]+)</CgiAction>");
                    if (!cgi.Success || cgi.Groups[1].Value != "waitmsg")
                    {
                        ready = true;
                        break;
                    }

                    // 서버가 TaskNo 갱신 시 반영
                    var tm2 = Regex.Match(lastWait.Body, @"<ParamValue>([^<]+)</ParamValue>");
                    if (tm2.Success) taskNo = tm2.Groups[1].Value;

                    var iv = Regex.Match(lastWait.Body, @"<Interval>(\d+)</Interval>");
                    var interval = iv.Success ? int.Parse(iv.Groups[1].Value) : 500;
                    await Task.Delay(Math.Min(interval, 2000));
                }

                if (!ready)
                    result.Logs.Add($"[신도D420파일][WARN] waitmsg 폴링 타임아웃, 최종 조회 시도");

                // Step 3: box_detail.xml 최종 조회
                var detailEx = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/box_detail.xml",
                    $"waitend=true&TaskNo={taskNo}&_=", $"{session.BaseUrl}/wcd/box_list.xml", result.Logs);

                if (!detailEx.IsSuccessStatusCode)
                {
                    result.Logs.Add("[신도D420파일][FAIL] box_detail.xml HTTP 실패");
                    if (lastWait != null) result.Logs.Add(lastWait.Dump());
                    result.Logs.Add(detailEx.Dump());
                    return DriverResult<List<BoxFile>>.Fail("파일 목록 수신 실패", result.Logs);
                }

                // 박스 불일치 검증 (엉뚱한 박스 응답이면 세션 무효화)
                var actualBoxId = ExtractDetailBoxId(detailEx.Body);
                if (actualBoxId != null && actualBoxId != target.boxId)
                {
                    result.Logs.Add($"[신도D420파일][WARN] 박스 불일치 — 요청={target.boxId}, 응답={actualBoxId}. 세션 무효화.");
                    result.Logs.Add(detailEx.Dump());
                    InvalidateSession(device.Ip);
                    return DriverResult<List<BoxFile>>.Fail($"박스 불일치 (응답={actualBoxId}, 요청={target.boxId})", result.Logs);
                }

                var files = ParseBoxFiles(detailEx.Body);
                result.Logs.Add($"[신도D420파일] 파일 {files.Count}개");
                result.Success = true;
                result.Data = files;
                result.Message = $"{files.Count}개 파일";
                return result;
            }
            finally
            {
                // usr 쿠키를 목록용(F_ULU)으로 원복해 이후 박스 CRUD 가 정상 동작하도록
                ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_ULU");
            }
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도D420파일][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            result.Success = false;
            result.Message = $"파일 목록 오류: {ex.Message}";
            return result;
        }
    }

    /// <summary>box_detail.xml 응답의 최상위 BoxInfo.BoxID 값.</summary>
    private static string? ExtractDetailBoxId(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var bi = doc.Descendants("BoxInfo").FirstOrDefault();
            return bi?.Element("BoxID")?.Value;
        }
        catch { return null; }
    }

    /// <summary>
    /// box_detail.xml 의 &lt;BoxJobInfo&gt; 블록에서 파일 목록 추출.
    /// DocId = BoxJobID, Name = JobName, ScannedAt = JobTime/CreateTime.
    /// </summary>
    private static List<BoxFile> ParseBoxFiles(string xml)
    {
        var files = new List<BoxFile>();
        if (string.IsNullOrWhiteSpace(xml)) return files;

        try
        {
            var doc = XDocument.Parse(xml);
            foreach (var job in doc.Descendants("BoxJobInfo"))
            {
                var file = new BoxFile
                {
                    DocId = job.Element("BoxJobID")?.Value ?? "",
                    Name = job.Element("JobName")?.Value ?? "",
                    PageCount = 1,
                    Size = "",
                };

                var createTime = job.Element("JobTime")?.Element("CreateTime");
                if (createTime != null)
                {
                    try
                    {
                        file.ScannedAt = new DateTime(
                            int.Parse(createTime.Element("Year")!.Value),
                            int.Parse(createTime.Element("Month")!.Value),
                            int.Parse(createTime.Element("Day")!.Value),
                            int.Parse(createTime.Element("Hour")!.Value),
                            int.Parse(createTime.Element("Minute")!.Value), 0);
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(file.DocId))
                    files.Add(file);
            }
        }
        catch
        {
            // XML 아닌 응답이면 빈 리스트 반환 — 호출부가 덤프로 확인
        }

        return files;
    }

    // ──────────────────────────────────────────────
    // DownloadFileAsync — 4단계 플로우 (D420_API.md 기반)
    //
    //   Step A: 박스 진입(PSL_F_UOUUser_BOX) → waitmsg → box_detail.xml
    //           (서버측 "박스 안, 해당 파일 있음" 컨텍스트 확보)
    //   Step B: POST user.cgi func=PSL_F_UOU_NXT           — 다운로드 설정 창 진입 (H_JLS=@1@, H_MAX=1)
    //   Step C: POST user.cgi func=PSL_F_UOU_DWN           — 다운로드 대기 (H_FMT=CompactPdf, H_JNL=\t{name}\t)
    //   Step D: GET  /wcd/progress                         — 응답 HTML 에서 <FORM id="A_DL" action="doc/...pdf"> 파싱,
    //                                                         갱신된 h_token/cginame1/cginame2/H_BAK/H_DLV 수집
    //   Step E: POST /wcd/doc/{filename}.pdf func=PSL_F_UOU_DLD — 실제 PDF 바이트 수신
    //
    // FileWatchService 가 신규 파일 감지 시 이 메서드를 자동 호출하여
    // box.LocalFolder\{file.Name}.pdf 에 저장한다.
    // ──────────────────────────────────────────────

    public override async Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file)
    {
        var result = new DriverResult<byte[]> { Logs = [] };
        try
        {
            result.Logs.Add($"[신도D420다운] {file.Name} (박스={box.Name})");

            var (session, logs) = await InitSessionAsync(device);
            result.Logs.AddRange(logs);
            if (session == null) return DriverResult<byte[]>.Fail("세션 실패", result.Logs);

            var (boxes, listEx) = await FetchBoxesAsync(session, result.Logs);
            if (!listEx.IsSuccessStatusCode)
            {
                result.Logs.Add("[신도D420다운][FAIL] 박스 목록 HTTP 실패");
                result.Logs.Add(listEx.Dump());
                return DriverResult<byte[]>.Fail("박스 목록 조회 실패", result.Logs);
            }
            var target = boxes.FirstOrDefault(b => b.name == box.Name);
            if (target == default)
            {
                result.Logs.Add($"[신도D420다운][FAIL] 박스 '{box.Name}' 없음 — 목록 덤프:");
                result.Logs.Add(listEx.Dump());
                return DriverResult<byte[]>.Fail($"박스 '{box.Name}' 찾을 수 없음", result.Logs);
            }

            var uri = new Uri(session.BaseUrl);
            var wcdUri = new Uri($"{session.BaseUrl}/wcd/");
            ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_UOU");

            try
            {
                var pw = box.Password ?? "";
                var pdfTab = "\t" + file.Name + "\t";
                var boxId = target.boxId;

                // ── Step A: 박스 진입 + 폴링 + box_detail.xml (서버 컨텍스트 확보)
                var enterPayload =
                    $"func=PSL_F_UOUUser_BOX" +
                    $"&H_BID={boxId}" +
                    $"&T_BID={boxId}" +
                    $"&P_BPA={Uri.EscapeDataString(pw)}" +
                    $"&h_token={Uri.EscapeDataString(session.Token)}" +
                    $"&H_PID=-1&H_IPA=On&_=";
                var enterEx = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/user.cgi",
                    enterPayload, $"{session.BaseUrl}/wcd/box_list.xml", result.Logs);
                if (!enterEx.IsSuccessStatusCode)
                {
                    result.Logs.Add("[신도D420다운][FAIL] 박스 진입 실패");
                    result.Logs.Add(enterEx.Dump());
                    return DriverResult<byte[]>.Fail("박스 진입 실패", result.Logs);
                }

                var taskNo = "0";
                var tmEnter = Regex.Match(enterEx.Body, @"<ParamValue>([^<]+)</ParamValue>");
                if (tmEnter.Success) taskNo = tmEnter.Groups[1].Value;

                for (var attempt = 0; attempt < 15; attempt++)
                {
                    var waitEx = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/waitmsg",
                        $"TaskNo={taskNo}&_=", $"{session.BaseUrl}/wcd/box_list.xml");
                    var cgi = Regex.Match(waitEx.Body, @"<CgiAction>([^<]+)</CgiAction>");
                    if (!cgi.Success || cgi.Groups[1].Value != "waitmsg") break;
                    var tm2 = Regex.Match(waitEx.Body, @"<ParamValue>([^<]+)</ParamValue>");
                    if (tm2.Success) taskNo = tm2.Groups[1].Value;
                    var iv = Regex.Match(waitEx.Body, @"<Interval>(\d+)</Interval>");
                    var interval = iv.Success ? int.Parse(iv.Groups[1].Value) : 500;
                    await Task.Delay(Math.Min(interval, 1000));
                }

                var detailEx = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/box_detail.xml",
                    $"waitend=true&TaskNo={taskNo}&_=", $"{session.BaseUrl}/wcd/box_list.xml", result.Logs);
                if (!detailEx.IsSuccessStatusCode)
                {
                    result.Logs.Add("[신도D420다운][FAIL] box_detail.xml 실패");
                    result.Logs.Add(detailEx.Dump());
                    return DriverResult<byte[]>.Fail("박스 상세 조회 실패", result.Logs);
                }

                // box_detail.xml에서 최신 토큰 갱신 (가능한 경우)
                var detailToken = ExtractToken(detailEx.Body);
                if (detailToken != null) session.Token = detailToken;

                // ── Step B: NXT (파일 다운로드 설정 창 진입)
                var nxtPayload =
                    $"func=PSL_F_UOU_NXT" +
                    $"&h_token={Uri.EscapeDataString(session.Token)}" +
                    $"&H_JLS={Uri.EscapeDataString("@1@")}" +
                    $"&H_CLS=" +
                    $"&H_PID=-1" +
                    $"&H_DTY=FileDownload" +
                    $"&H_XTP=Thumbnail" +
                    $"&H_TAB=" +
                    $"&H_BOX={boxId}" +
                    $"&H_BPA=" +
                    $"&H_BTY=User" +
                    $"&H_OPE=FileDownload" +
                    $"&H_MAX=1";
                var nxtEx = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/user.cgi",
                    nxtPayload, $"{session.BaseUrl}/wcd/box_detail.xml", result.Logs);
                if (!nxtEx.IsSuccessStatusCode)
                {
                    result.Logs.Add("[신도D420다운][FAIL] NXT(설정) 실패");
                    result.Logs.Add(nxtEx.Dump());
                    return DriverResult<byte[]>.Fail("다운로드 설정 실패", result.Logs);
                }
                // NXT 응답에서 토큰 갱신
                var nxtToken = ExtractToken(nxtEx.Body);
                if (nxtToken != null) session.Token = nxtToken;

                // ── Step C: DWN (다운로드 대기 창 진입)
                var dwnPayload =
                    $"H_TAB=" +
                    $"&func=PSL_F_UOU_DWN" +
                    $"&h_token={Uri.EscapeDataString(session.Token)}" +
                    $"&H_BOX={boxId}" +
                    $"&H_BPA=" +
                    $"&H_BTY=User" +
                    $"&H_PID=-1" +
                    $"&H_DTY=FileDownload" +
                    $"&H_XTP=Thumbnail" +
                    $"&H_FMT=CompactPdf" +
                    $"&H_JLS={Uri.EscapeDataString("@1@")}" +
                    $"&H_JNL={Uri.EscapeDataString(pdfTab)}" +
                    $"&H_JOR={Uri.EscapeDataString("@1@")}" +
                    $"&H_DCN=1" +
                    $"&C_GFA=On" +
                    $"&C_SET=C_SET" +
                    $"&F_UOU_S_FOR=CompactPdf" +
                    $"&S_OUT=Off" +
                    $"&F_UOU_R_PAG=MultiPage" +
                    $"&R_SPG=Off";
                var dwnEx = await PostFormAsync(session.Client, $"{session.BaseUrl}/wcd/user.cgi",
                    dwnPayload, $"{session.BaseUrl}/wcd/user.cgi", result.Logs);
                if (!dwnEx.IsSuccessStatusCode)
                {
                    result.Logs.Add("[신도D420다운][FAIL] DWN(대기) 실패");
                    result.Logs.Add(dwnEx.Dump());
                    return DriverResult<byte[]>.Fail("다운로드 준비 실패", result.Logs);
                }

                // ── Step D: progress 폴링 — 응답 HTML에서 다운로드 form 추출
                string? actionUrl = null;
                string dlToken = session.Token;
                string? cginame1 = null, cginame2 = null;
                string hBak = "0", hDlv = "";
                HttpExchange? lastProg = null;

                for (var attempt = 0; attempt < 30; attempt++)
                {
                    var progEx = await GetAsync(session.Client, $"{session.BaseUrl}/wcd/progress");
                    lastProg = progEx;
                    if (progEx.IsSuccessStatusCode)
                    {
                        var body = progEx.Body;
                        var m = Regex.Match(body, @"<FORM[^>]*id=[""']A_DL[""'][^>]*action=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            actionUrl = m.Groups[1].Value;
                            var t = ExtractToken(body);
                            if (t != null) dlToken = t;
                            cginame1 = ExtractInputValue(body, "cginame1");
                            cginame2 = ExtractInputValue(body, "cginame2");
                            hBak = ExtractInputValue(body, "H_BAK") ?? "0";
                            hDlv = ExtractInputValue(body, "H_DLV") ?? "";
                            result.Logs.Add($"[신도D420다운] progress action={actionUrl}");
                            break;
                        }
                    }
                    await Task.Delay(800);
                }

                if (actionUrl == null)
                {
                    result.Logs.Add("[신도D420다운][FAIL] progress 페이지에서 다운로드 form 미발견 — 최종 응답 덤프:");
                    if (lastProg != null) result.Logs.Add(lastProg.Dump());
                    return DriverResult<byte[]>.Fail("다운로드 준비 타임아웃", result.Logs);
                }

                // ── Step E: 실제 PDF 수신 — action="doc/{name}.pdf" 는 /wcd/ 기준 상대경로
                var absUrl = actionUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? actionUrl
                    : $"{session.BaseUrl}/wcd/{actionUrl.TrimStart('/')}";
                var defaultName = $"doc/{file.Name}.pdf";
                var dlPayload =
                    $"func=PSL_F_UOU_DLD" +
                    $"&h_token={Uri.EscapeDataString(dlToken)}" +
                    $"&cginame1={Uri.EscapeDataString(cginame1 ?? defaultName)}" +
                    $"&cginame2={Uri.EscapeDataString(cginame2 ?? defaultName)}" +
                    $"&H_BAK={Uri.EscapeDataString(hBak)}" +
                    $"&H_TAB=" +
                    $"&H_DLV={Uri.EscapeDataString(hDlv)}";

                var pdfReq = new HttpRequestMessage(HttpMethod.Post, absUrl)
                {
                    Content = new StringContent(dlPayload, Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                pdfReq.Headers.Add("Referer", $"{session.BaseUrl}/wcd/progress");
                pdfReq.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

                result.Logs.Add($"[HTTP→] POST ({absUrl})");
                var (pdfEx, bytes) = await HttpDiagnostics.SendBytesAsync(session.Client, pdfReq, dlPayload);
                result.Logs.Add($"[HTTP←] {pdfEx.StatusCode} {pdfEx.ReasonPhrase} ({bytes.Length}바이트, {(int)pdfEx.Elapsed.TotalMilliseconds}ms)");

                if (!pdfEx.IsSuccessStatusCode)
                {
                    result.Logs.Add($"[신도D420다운][FAIL] PDF HTTP {pdfEx.StatusCode} — 덤프:");
                    result.Logs.Add(pdfEx.Dump());
                    return DriverResult<byte[]>.Fail($"PDF HTTP {pdfEx.StatusCode}", result.Logs);
                }

                // %PDF 매직 넘버 검증
                var isPdf = bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;
                if (!isPdf)
                {
                    result.Logs.Add("[신도D420다운][FAIL] PDF 매직넘버 불일치 — 서버가 HTML/에러 반환한 듯:");
                    result.Logs.Add(pdfEx.Dump());
                    return DriverResult<byte[]>.Fail("PDF 응답 아님", result.Logs);
                }

                result.Logs.Add($"[신도D420다운] 완료 {bytes.Length} bytes");
                result.Success = true;
                result.Data = bytes;
                result.Message = $"{bytes.Length} bytes";
                return result;
            }
            finally
            {
                // 박스 CRUD 가 정상 동작하도록 usr 쿠키 원복
                ReplaceUsrCookie(session.Cookies, uri, wcdUri, "F_ULU");
            }
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[신도D420다운][ERROR] {ex.Message}");
            result.Logs.Add($"[STACK] {ex.StackTrace}");
            return DriverResult<byte[]>.Fail($"다운로드 오류: {ex.Message}", result.Logs);
        }
    }

    /// <summary>HTML 의 &lt;input name="X" value="Y"&gt; 에서 value 추출. 속성 순서 무관.</summary>
    private static string? ExtractInputValue(string html, string name)
    {
        var esc = Regex.Escape(name);
        var a = Regex.Match(html, $@"name\s*=\s*[""']{esc}[""'][^>]*\svalue\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
        if (a.Success) return a.Groups[1].Value;
        var b = Regex.Match(html, $@"value\s*=\s*[""']([^""']*)[""'][^>]*\sname\s*=\s*[""']{esc}[""']", RegexOptions.IgnoreCase);
        return b.Success ? b.Groups[1].Value : null;
    }
}
