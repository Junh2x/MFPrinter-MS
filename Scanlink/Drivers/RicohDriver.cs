using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Drivers;

/// <summary>
/// 리코 복합기 드라이버.
/// 문서서버(Web Image Monitor)를 통한 폴더 CRUD.
/// </summary>
public class RicohDriver : IMfpDriver
{
    public MfpBrand Brand => MfpBrand.Ricoh;

    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36";

    // ──────────────────────────────────────────────
    // 세션 헬퍼
    // ──────────────────────────────────────────────

    private static string B64(string text) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    private static string? ExtractWimToken(string html)
    {
        var m = Regex.Match(html, @"name=[""']wimToken[""'][^>]*value=[""'](\d+)");
        return m.Success ? m.Groups[1].Value : null;
    }

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
        return (client, cookies);
    }

    /// <summary>리코 웹 로그인 → (client, wimToken, baseUrl)</summary>
    private static async Task<(HttpClient? client, string? wimToken, string baseUrl, List<string> logs)>
        LoginAsync(MfpDevice device)
    {
        var logs = new List<string>();
        var baseUrl = string.IsNullOrEmpty(device.BaseUrl) ? $"http://{device.Ip}" : device.BaseUrl;
        var (client, cookies) = CreateClient();

        try
        {
            // 초기 쿠키
            logs.Add($"[리코] 접속: {baseUrl}");
            await client.GetAsync($"{baseUrl}/");
            cookies.Add(new Uri(baseUrl), new Cookie("cookieOnOffChecker", "on"));

            // wimToken 추출
            var authHtml = await (await client.GetAsync(
                $"{baseUrl}/web/guest/ko/websys/webArch/authForm.cgi")).Content.ReadAsStringAsync();
            var wimToken = ExtractWimToken(authHtml);
            if (wimToken == null)
            {
                logs.Add("[리코][FAIL] wimToken 추출 실패");
                client.Dispose();
                return (null, null, baseUrl, logs);
            }

            // 로그인
            logs.Add("[리코] 로그인 중...");
            var loginData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["wimToken"] = wimToken,
                ["userid_work"] = "",
                ["userid"] = B64("admin"),
                ["password_work"] = "",
                ["password"] = B64(""),
                ["open"] = "",
            });
            var req = new HttpRequestMessage(HttpMethod.Post,
                $"{baseUrl}/web/guest/ko/websys/webArch/login.cgi") { Content = loginData };
            req.Headers.Add("Referer", $"{baseUrl}/web/guest/ko/websys/webArch/authForm.cgi");
            await client.SendAsync(req);

            var allCookies = cookies.GetAllCookies();
            var hasSession = allCookies.Any(c => c.Name is "wimsesid" or "risessionid");
            if (!hasSession)
            {
                logs.Add("[리코][FAIL] 로그인 실패 — 세션 쿠키 없음");
                client.Dispose();
                return (null, null, baseUrl, logs);
            }

            logs.Add("[리코] 로그인 성공");
            device.BaseUrl = baseUrl;
            return (client, wimToken, baseUrl, logs);
        }
        catch (Exception ex)
        {
            logs.Add($"[리코][ERROR] 로그인: {ex.Message}");
            client.Dispose();
            return (null, null, baseUrl, logs);
        }
    }

    /// <summary>폴더 목록 조회 → (wimToken, folders)</summary>
    private static async Task<(string? wimToken, List<(string id, string name)> folders)>
        ListFoldersAsync(HttpClient client, string baseUrl, List<string> logs)
    {
        var req = new HttpRequestMessage(HttpMethod.Get,
            $"{baseUrl}/web/entry/ko/webdocbox/folderListPage.cgi");
        req.Headers.Add("Referer", $"{baseUrl}/web/entry/ko/websys/webArch/topPage.cgi");
        var html = await (await client.SendAsync(req)).Content.ReadAsStringAsync();

        var wimToken = ExtractWimToken(html);

        var folders = new List<(string id, string name)>();
        foreach (Match m in Regex.Matches(html,
            @"docListPage\.cgi\?selectedFolderId=(\d+)[^>]*>([^<]+)</a>"))
        {
            folders.Add((m.Groups[1].Value, m.Groups[2].Value.Trim()));
        }

        logs.Add($"[리코] 폴더 {folders.Count}개: {string.Join(", ", folders.Select(f => f.name))}");
        return (wimToken, folders);
    }

    /// <summary>빈 폴더 ID 찾기 (001~200 중 사용 안 된 번호)</summary>
    private static string FindEmptyFolderId(List<(string id, string name)> folders)
    {
        var usedIds = folders.Select(f => f.id).ToHashSet();
        for (var i = 1; i <= 200; i++)
        {
            var id = i.ToString();
            if (!usedIds.Contains(id))
                return id.PadLeft(3, '0');
        }
        return "";
    }

    private static async Task<string> PostFormAsync(HttpClient client, string url,
        Dictionary<string, string> data, string referer)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(data)
        };
        req.Headers.Add("Referer", referer);
        req.Headers.Add("Upgrade-Insecure-Requests", "1");
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
        string? wimToken; List<string> logs;
        (client, wimToken, _, logs) = await LoginAsync(device);
        result.Logs.AddRange(logs);

        if (client == null)
            return DriverResult.Fail("연결 실패", result.Logs);

        device.Status = ConnectionStatus.Connected;
        result.Success = true;
        result.Message = "연결 성공";
        return result;
        } catch (Exception ex) {
            result.Logs.Add($"[리코][ERROR] {ex.Message}");
            return DriverResult.Fail($"연결 오류: {ex.Message}", result.Logs);
        } finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // SetupAsync — 리코는 별도 설정 불필요
    // ──────────────────────────────────────────────

    public Task<DriverResult> SetupAsync(MfpDevice device)
    {
        device.IsConfigured = true;
        return Task.FromResult(DriverResult.Ok("리코는 별도 초기 설정 불필요"));
    }

    // ──────────────────────────────────────────────
    // GetScanBoxListAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device)
    {
        var result = new DriverResult<List<ScanBox>> { Logs = [] };
        HttpClient? client = null;
        try {
        string? wimToken; string baseUrl; List<string> loginLogs;
        (client, wimToken, baseUrl, loginLogs) = await LoginAsync(device);
        result.Logs.AddRange(loginLogs);
        if (client == null) return DriverResult<List<ScanBox>>.Fail("로그인 실패", result.Logs);

        var (_, folders) = await ListFoldersAsync(client, baseUrl, result.Logs);

        var boxes = folders.Select(f => new ScanBox
        {
            Name = f.name,
            MfpDeviceId = device.Id,
        }).ToList();

        result.Success = true;
        result.Data = boxes;
        result.Message = $"{boxes.Count}개 조회";
        return result;
        } catch (Exception ex) {
            result.Logs.Add($"[리코][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"조회 오류: {ex.Message}";
            return result;
        } finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // AddScanBoxAsync — 문서서버 폴더 생성 (5단계)
    // ──────────────────────────────────────────────

    public async Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try {

        result.Logs.Add($"[리코추가] 스캔함 추가: {box.Name}");

        string? wimToken; string baseUrl; List<string> loginLogs;
        (client, wimToken, baseUrl, loginLogs) = await LoginAsync(device);
        result.Logs.AddRange(loginLogs);
        if (client == null || wimToken == null)
            return DriverResult.Fail("로그인 실패", result.Logs);

        // 폴더 목록 → wimToken 갱신 + 빈 ID 찾기
        var (newToken, folders) = await ListFoldersAsync(client, baseUrl, result.Logs);
        if (newToken != null) wimToken = newToken;

        var folderId = FindEmptyFolderId(folders);
        if (string.IsNullOrEmpty(folderId))
            return DriverResult.Fail("빈 폴더 번호가 없습니다.", result.Logs);

        result.Logs.Add($"[리코추가] 폴더 ID={folderId}, 이름={box.Name}");

        var now = DateTime.Now;
        var pw = box.Password ?? "";
        var pwB64 = !string.IsNullOrEmpty(pw) ? B64(pw) : "";

        // Step 1: 폴더 생성 폼 진입
        result.Logs.Add("[리코추가] Step1: 생성 폼...");
        await PostFormAsync(client,
            $"{baseUrl}/web/entry/ko/webdocbox/folderPropPage.cgi",
            new() {
                ["wimToken"] = wimToken, ["mode"] = "CREATE", ["selectedDocIds"] = "",
                ["subReturnDsp"] = "", ["useInputParam"] = "", ["useSavedPropParam"] = "false",
                ["_hour"] = now.ToString("HH"), ["_min"] = now.ToString("mm"),
            },
            $"{baseUrl}/web/entry/ko/webdocbox/folderListPage.cgi");

        // Step 2: 비밀번호 페이지
        result.Logs.Add("[리코추가] Step2: 비밀번호 페이지...");
        await PostFormAsync(client,
            $"{baseUrl}/web/entry/ko/webdocbox/chPasswordPage.cgi",
            new() {
                ["wimToken"] = wimToken, ["targetFolderId"] = folderId,
                ["changedFolderName"] = box.Name, ["mode"] = "CREATE",
                ["targetDocId"] = folderId, ["selectedFolderId"] = "",
                ["title"] = box.Name, ["useSavedPropParam"] = "true",
                ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
            },
            $"{baseUrl}/web/entry/ko/webdocbox/folderPropPage.cgi");

        // Step 3: 비밀번호 설정
        result.Logs.Add("[리코추가] Step3: 비밀번호 설정...");
        await PostFormAsync(client,
            $"{baseUrl}/web/entry/ko/webdocbox/commitChPassword.cgi",
            new() {
                ["wimToken"] = wimToken, ["title"] = box.Name,
                ["creator"] = "", ["dataFormat"] = "", ["allPages"] = "false",
                ["cid"] = "", ["convBW"] = "", ["backUp"] = "",
                ["backUpFormatStr"] = "", ["backUpResoStr"] = "",
                ["targetDocId"] = folderId, ["oldPassword"] = "undefined",
                ["newPassword"] = pwB64, ["confirmation"] = pwB64,
                ["useInputParam"] = "false", ["useSavedParam"] = "",
                ["subReturnDsp"] = "3", ["mode"] = "CREATE", ["wayTo"] = "",
                ["useSavedPropParam"] = "true", ["selectedFolderId"] = "",
                ["ID"] = "", ["dummy"] = "",
            },
            $"{baseUrl}/web/entry/ko/webdocbox/chPasswordPage.cgi");

        // Step 4: 속성 페이지
        result.Logs.Add("[리코추가] Step4: 속성 확인...");
        await PostFormAsync(client,
            $"{baseUrl}/web/entry/ko/webdocbox/folderPropPage.cgi",
            new() {
                ["wimToken"] = wimToken,
                ["id"] = "", ["jt"] = "", ["el"] = "",
                ["urlLang"] = "ko", ["urlProfile"] = "entry",
                ["pdfThumbnailURI"] = "", ["thumbnailURI"] = "", ["WidthSize"] = "",
                ["subdocCount"] = "", ["targetDocId"] = folderId,
                ["title"] = "", ["creator"] = "",
                ["useInputParam"] = "false", ["useSavedParam"] = "",
                ["subReturnDsp"] = "3", ["mode"] = "CREATE", ["wayTo"] = "",
                ["selectedFolderId"] = folderId, ["useSavedPropParam"] = "true",
                ["ID"] = "", ["simpleErrorMessage"] = "", ["dummy"] = "",
            },
            $"{baseUrl}/web/entry/ko/webdocbox/commitChPassword.cgi");

        // Step 5: 최종 생성
        result.Logs.Add("[리코추가] Step5: 생성 확정...");
        var html = await PostFormAsync(client,
            $"{baseUrl}/web/entry/ko/webdocbox/putFolderProp.cgi",
            new() {
                ["wimToken"] = wimToken, ["targetFolderId"] = folderId,
                ["changedFolderName"] = box.Name, ["mode"] = "CREATE",
                ["targetDocId"] = "", ["selectedFolderId"] = "",
                ["title"] = "", ["useSavedPropParam"] = "true",
                ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
            },
            $"{baseUrl}/web/entry/ko/webdocbox/folderPropPage.cgi");

        var errMatch = Regex.Match(html, @"simpleErrorMessage[^>]*value=[""']([^""']+)");
        if (errMatch.Success && !string.IsNullOrWhiteSpace(errMatch.Groups[1].Value))
        {
            result.Logs.Add($"[리코추가][FAIL] 서버 에러: {errMatch.Groups[1].Value}");
            return DriverResult.Fail($"생성 실패: {errMatch.Groups[1].Value}", result.Logs);
        }

        result.Logs.Add($"[리코추가] 생성 완료! ID={folderId}");
        result.Success = true;
        result.Message = "스캔함 추가 완료";
        return result;

        } catch (Exception ex) {
            result.Logs.Add($"[리코추가][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"추가 오류: {ex.Message}";
            return result;
        } finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // DeleteScanBoxAsync — 문서서버 폴더 삭제
    // ──────────────────────────────────────────────

    public async Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try {

        result.Logs.Add($"[리코삭제] 스캔함 삭제: {box.Name}");

        string? wimToken; string baseUrl; List<string> loginLogs;
        (client, wimToken, baseUrl, loginLogs) = await LoginAsync(device);
        result.Logs.AddRange(loginLogs);
        if (client == null || wimToken == null)
            return DriverResult.Fail("로그인 실패", result.Logs);

        // 폴더 목록에서 해당 폴더 ID 찾기
        var (newToken, folders) = await ListFoldersAsync(client, baseUrl, result.Logs);
        if (newToken != null) wimToken = newToken;

        var target = folders.FirstOrDefault(f => f.name == box.Name);
        if (target == default)
        {
            result.Logs.Add($"[리코삭제] 폴더 '{box.Name}' 찾을 수 없음 (이미 삭제됨)");
            result.Success = true;
            result.Message = "삭제 완료 (이미 없음)";
            return result;
        }

        result.Logs.Add($"[리코삭제] 대상 폴더 ID={target.id}");

        var now = DateTime.Now;

        // Step 1: 삭제 확인 페이지 (folderDeletePage.cgi)
        result.Logs.Add("[리코삭제] Step1: 삭제 확인 페이지...");
        var html = await PostFormAsync(client,
            $"{baseUrl}/web/entry/ko/webdocbox/folderDeletePage.cgi",
            new() {
                ["wimToken"] = wimToken,
                ["mode"] = "",
                ["selectedDocIds"] = "",
                ["subReturnDsp"] = "",
                ["useInputParam"] = "",
                ["useSavedPropParam"] = "false",
                ["_hour"] = now.ToString("HH"),
                ["_min"] = now.ToString("mm"),
                ["selectedFolderId"] = target.id,
            },
            $"{baseUrl}/web/entry/ko/webdocbox/folderListPage.cgi");

        var t = ExtractWimToken(html); if (t != null) wimToken = t;
        result.Logs.Add($"[리코삭제] Step1 응답: {html.Length}자, wimToken갱신={t != null}");

        // Step 2: 삭제 확정 (deleteFolders.cgi)
        result.Logs.Add("[리코삭제] Step2: 삭제 확정...");
        html = await PostFormAsync(client,
            $"{baseUrl}/web/entry/ko/webdocbox/deleteFolders.cgi",
            new() {
                ["wimToken"] = wimToken,
                ["selectedFolderId"] = target.id,
                ["subReturnDsp"] = "3",
            },
            $"{baseUrl}/web/entry/ko/webdocbox/folderDeletePage.cgi");

        result.Logs.Add($"[리코삭제] Step2 응답: {html.Length}자");

        var errMatch = Regex.Match(html, @"simpleErrorMessage[^>]*value=[""']([^""']+)");
        if (errMatch.Success && !string.IsNullOrWhiteSpace(errMatch.Groups[1].Value))
        {
            result.Logs.Add($"[리코삭제][FAIL] 서버 에러: {errMatch.Groups[1].Value}");
            return DriverResult.Fail($"삭제 실패: {errMatch.Groups[1].Value}", result.Logs);
        }

        result.Logs.Add("[리코삭제] 삭제 완료");
        result.Success = true;
        result.Message = "삭제 완료";
        return result;

        } catch (Exception ex) {
            result.Logs.Add($"[리코삭제][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"삭제 오류: {ex.Message}";
            return result;
        } finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // UpdateScanBoxAsync — 문서서버 폴더 수정 (이름/비밀번호)
    // ──────────────────────────────────────────────

    public async Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try {

        var searchName = oldName ?? box.Name;
        result.Logs.Add($"[리코수정] 스캔함 수정: {searchName} → {box.Name}");

        string? wimToken; string baseUrl; List<string> loginLogs;
        (client, wimToken, baseUrl, loginLogs) = await LoginAsync(device);
        result.Logs.AddRange(loginLogs);
        if (client == null || wimToken == null)
            return DriverResult.Fail("로그인 실패", result.Logs);

        // 폴더 목록에서 대상 찾기
        var (newToken, folders) = await ListFoldersAsync(client, baseUrl, result.Logs);
        if (newToken != null) wimToken = newToken;

        var target = folders.FirstOrDefault(f => f.name == searchName);
        if (target == default)
            return DriverResult.Fail($"폴더 '{searchName}'을 찾을 수 없습니다.", result.Logs);

        result.Logs.Add($"[리코수정] 대상 폴더 ID={target.id}");

        var now = DateTime.Now;
        var pw = box.Password ?? "";
        var changePw = !string.IsNullOrEmpty(pw);
        string html;

        // Step 1: 수정 페이지 진입 (mode=PROPERTY, /web/guest/ko/)
        result.Logs.Add("[리코수정] Step1: 속성 페이지 진입...");
        html = await PostFormAsync(client,
            $"{baseUrl}/web/guest/ko/webdocbox/folderPropPage.cgi",
            new() {
                ["wimToken"] = wimToken, ["mode"] = "PROPERTY",
                ["selectedDocIds"] = "", ["subReturnDsp"] = "",
                ["useInputParam"] = "", ["useSavedPropParam"] = "false",
                ["_hour"] = now.ToString("HH"), ["_min"] = now.ToString("mm"),
                ["selectedFolderId"] = target.id,
            },
            $"{baseUrl}/web/guest/ko/webdocbox/folderListPage.cgi");
        var t = ExtractWimToken(html); if (t != null) wimToken = t;
        result.Logs.Add($"[리코수정] Step1 응답: {html.Length}자");

        // Step 2: 비밀번호 변경 페이지 (mode=PROPERTY)
        result.Logs.Add("[리코수정] Step2: 비밀번호 페이지...");
        html = await PostFormAsync(client,
            $"{baseUrl}/web/guest/ko/webdocbox/chPasswordPage.cgi",
            new() {
                ["wimToken"] = wimToken, ["targetFolderId"] = target.id,
                ["changedFolderName"] = box.Name, ["mode"] = "PROPERTY",
                ["targetDocId"] = target.id, ["selectedFolderId"] = "",
                ["title"] = box.Name, ["useSavedPropParam"] = "true",
                ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
            },
            $"{baseUrl}/web/guest/ko/webdocbox/folderPropPage.cgi");
        t = ExtractWimToken(html); if (t != null) wimToken = t;
        result.Logs.Add($"[리코수정] Step2 응답: {html.Length}자");

        // Step 3: 비밀번호 저장 (oldPassword 빈값)
        var pwB64 = changePw ? B64(pw) : "";
        result.Logs.Add("[리코수정] Step3: 비밀번호 저장...");
        html = await PostFormAsync(client,
            $"{baseUrl}/web/guest/ko/webdocbox/commitChPassword.cgi",
            new() {
                ["wimToken"] = wimToken, ["title"] = box.Name,
                ["creator"] = "", ["dataFormat"] = "", ["allPages"] = "false",
                ["cid"] = "", ["convBW"] = "", ["backUp"] = "",
                ["backUpFormatStr"] = "", ["backUpResoStr"] = "",
                ["targetDocId"] = target.id,
                ["oldPassword"] = !string.IsNullOrEmpty(oldPassword) ? B64(oldPassword) : "",
                ["newPassword"] = pwB64, ["confirmation"] = pwB64,
                ["useInputParam"] = "false", ["useSavedParam"] = "",
                ["subReturnDsp"] = "3", ["mode"] = "PROPERTY", ["wayTo"] = "",
                ["useSavedPropParam"] = "true", ["selectedFolderId"] = "",
                ["ID"] = "", ["dummy"] = "",
            },
            $"{baseUrl}/web/guest/ko/webdocbox/chPasswordPage.cgi");
        t = ExtractWimToken(html); if (t != null) wimToken = t;
        result.Logs.Add($"[리코수정] Step3 응답: {html.Length}자");

        // Step 4: 최종 저장 (mode=PROPERTY)
        result.Logs.Add("[리코수정] Step4: 수정 확정...");
        html = await PostFormAsync(client,
            $"{baseUrl}/web/guest/ko/webdocbox/putFolderProp.cgi",
            new() {
                ["wimToken"] = wimToken, ["targetFolderId"] = target.id,
                ["changedFolderName"] = box.Name, ["mode"] = "PROPERTY",
                ["targetDocId"] = "", ["selectedFolderId"] = "",
                ["title"] = "", ["useSavedPropParam"] = "true",
                ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
            },
            $"{baseUrl}/web/guest/ko/webdocbox/folderPropPage.cgi");
        result.Logs.Add($"[리코수정] Step4 응답: {html.Length}자");

        var errMatch = Regex.Match(html, @"simpleErrorMessage[^>]*value=[""']([^""']+)");
        if (errMatch.Success && !string.IsNullOrWhiteSpace(errMatch.Groups[1].Value))
        {
            result.Logs.Add($"[리코수정][FAIL] 서버 에러: {errMatch.Groups[1].Value}");
            return DriverResult.Fail($"수정 실패: {errMatch.Groups[1].Value}", result.Logs);
        }

        result.Logs.Add("[리코수정] 수정 완료");
        result.Success = true;
        result.Message = "수정 완료";
        return result;

        } catch (Exception ex) {
            result.Logs.Add($"[리코수정][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"수정 오류: {ex.Message}";
            return result;
        } finally { client?.Dispose(); }
    }
}
