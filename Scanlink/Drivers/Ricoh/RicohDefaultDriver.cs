using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Drivers.Ricoh;

/// <summary>
/// 리코 복합기 기본 드라이버.
/// 문서서버(Web Image Monitor)를 통한 폴더 CRUD.
///
/// 현재는 로그인/플로우를 매 호출마다 수행하므로 세션 캐시 없음.
/// 프로필(entry/guest)만 IP 단위로 캐시.
/// </summary>
public sealed class RicohDefaultDriver : RicohDriverBase
{
    /// <summary>
    /// 기기별 WIM 프로필 캐시 (entry=관리자, guest=사용자).
    /// 모델에 따라 지원 프로필이 달라 자동 감지 후 기억.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _profileCache = new();

    public override void DisposeSessions()
    {
        _profileCache.Clear();
    }

    // ──────────────────────────────────────────────
    // 로그인
    // ──────────────────────────────────────────────

    private static async Task<(HttpClient? client, string? wimToken, string baseUrl, List<string> logs)>
        LoginAsync(MfpDevice device)
    {
        var logs = new List<string>();
        var baseUrl = string.IsNullOrEmpty(device.BaseUrl) ? $"http://{device.Ip}" : device.BaseUrl;
        var (client, cookies) = CreateClient();

        try
        {
            logs.Add($"[리코] 접속: {baseUrl}");
            await client.GetAsync($"{baseUrl}/");
            cookies.Add(new Uri(baseUrl), new Cookie("cookieOnOffChecker", "on"));

            var authHtml = await (await client.GetAsync(
                $"{baseUrl}/web/guest/ko/websys/webArch/authForm.cgi")).Content.ReadAsStringAsync();
            var wimToken = ExtractWimToken(authHtml);
            if (wimToken == null)
            {
                logs.Add("[리코][FAIL] wimToken 추출 실패");
                client.Dispose();
                return (null, null, baseUrl, logs);
            }

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

    // ──────────────────────────────────────────────
    // 폴더 조회/프로필 감지
    // ──────────────────────────────────────────────

    private static async Task<(string profile, string? wimToken, List<(string id, string name)> folders)>
        ListFoldersAsync(HttpClient client, string baseUrl, string deviceIp, List<string> logs)
    {
        var cached = _profileCache.TryGetValue(deviceIp, out var cp) ? cp : null;
        var candidates = cached != null ? new[] { cached } : new[] { "entry", "guest" };

        foreach (var profile in candidates)
        {
            var req = new HttpRequestMessage(HttpMethod.Get,
                $"{baseUrl}/web/{profile}/ko/webdocbox/folderListPage.cgi");
            req.Headers.Add("Referer", $"{baseUrl}/web/{profile}/ko/websys/webArch/topPage.cgi");
            var html = await (await client.SendAsync(req)).Content.ReadAsStringAsync();
            var wimToken = ExtractWimToken(html);

            var folders = new List<(string id, string name)>();
            foreach (Match m in Regex.Matches(html,
                @"docListPage\.cgi\?selectedFolderId=(\d+)[^>]*>([^<]+)</a>"))
            {
                folders.Add((m.Groups[1].Value, m.Groups[2].Value.Trim()));
            }

            var isFolderPage = wimToken != null
                && (folders.Count > 0 || html.Contains("folderList_add") || html.Contains("folderListFormSubmit"));

            if (isFolderPage)
            {
                _profileCache[deviceIp] = profile;
                logs.Add($"[리코] profile={profile}, 폴더 {folders.Count}개: {string.Join(", ", folders.Select(f => f.name))}");
                return (profile, wimToken, folders);
            }

            logs.Add($"[리코] profile={profile} 불일치 — 다음 후보 시도 (wimToken={(wimToken != null)}, 폴더={folders.Count})");
        }

        logs.Add("[리코][FAIL] 프로필 감지 실패 — entry/guest 모두 유효한 folderListPage 반환 안 함");
        return ("entry", null, new List<(string id, string name)>());
    }

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

    // ──────────────────────────────────────────────
    // ConnectAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> ConnectAsync(MfpDevice device)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try
        {
            string? wimToken; List<string> logs;
            (client, wimToken, _, logs) = await LoginAsync(device);
            result.Logs.AddRange(logs);

            if (client == null)
                return DriverResult.Fail("연결 실패", result.Logs);

            device.Status = ConnectionStatus.Connected;
            result.Success = true;
            result.Message = "연결 성공";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[리코][ERROR] {ex.Message}");
            return DriverResult.Fail($"연결 오류: {ex.Message}", result.Logs);
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // GetScanBoxListAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device)
    {
        var result = new DriverResult<List<ScanBox>> { Logs = [] };
        HttpClient? client = null;
        try
        {
            string? wimToken; string baseUrl; List<string> loginLogs;
            (client, wimToken, baseUrl, loginLogs) = await LoginAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null) return DriverResult<List<ScanBox>>.Fail("로그인 실패", result.Logs);

            var (_, _, folders) = await ListFoldersAsync(client, baseUrl, device.Ip, result.Logs);

            var boxes = folders.Select(f => new ScanBox
            {
                Name = f.name,
                MfpDeviceId = device.Id,
            }).ToList();

            result.Success = true;
            result.Data = boxes;
            result.Message = $"{boxes.Count}개 조회";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[리코][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"조회 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // AddScanBoxAsync — 문서서버 폴더 생성 (5단계)
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try
        {
            result.Logs.Add($"[리코추가] 스캔함 추가: {box.Name}");

            string? wimToken; string baseUrl; List<string> loginLogs;
            (client, wimToken, baseUrl, loginLogs) = await LoginAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null || wimToken == null)
                return DriverResult.Fail("로그인 실패", result.Logs);

            var (profile, newToken, folders) = await ListFoldersAsync(client, baseUrl, device.Ip, result.Logs);
            if (newToken != null) wimToken = newToken;
            if (newToken == null)
                return DriverResult.Fail("폴더 목록 접근 실패 (프로필 감지 불가)", result.Logs);

            var folderId = FindEmptyFolderId(folders);
            if (string.IsNullOrEmpty(folderId))
                return DriverResult.Fail("빈 폴더 번호가 없습니다.", result.Logs);

            result.Logs.Add($"[리코추가] 폴더 ID={folderId}, 이름={box.Name}, profile={profile}");

            var now = DateTime.Now;
            var pw = box.Password ?? "";
            var pwB64 = !string.IsNullOrEmpty(pw) ? B64(pw) : "";
            var wb = $"{baseUrl}/web/{profile}/ko/webdocbox";

            // Step 1
            result.Logs.Add("[리코추가] Step1: 생성 폼...");
            var s1 = await PostFormAsync(client, $"{wb}/folderPropPage.cgi",
                new() {
                    ["wimToken"] = wimToken, ["mode"] = "CREATE", ["selectedDocIds"] = "",
                    ["subReturnDsp"] = "", ["useInputParam"] = "", ["useSavedPropParam"] = "false",
                    ["_hour"] = now.ToString("HH"), ["_min"] = now.ToString("mm"),
                },
                $"{wb}/folderListPage.cgi");
            var t1 = ExtractWimToken(s1); if (t1 != null) wimToken = t1;
            result.Logs.Add($"[리코추가] Step1 응답: {s1.Length}자");

            // Step 2
            result.Logs.Add("[리코추가] Step2: 비밀번호 페이지...");
            var s2 = await PostFormAsync(client, $"{wb}/chPasswordPage.cgi",
                new() {
                    ["wimToken"] = wimToken, ["targetFolderId"] = folderId,
                    ["changedFolderName"] = box.Name, ["mode"] = "CREATE",
                    ["targetDocId"] = folderId, ["selectedFolderId"] = "",
                    ["title"] = box.Name, ["useSavedPropParam"] = "true",
                    ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
                },
                $"{wb}/folderPropPage.cgi");
            var t2 = ExtractWimToken(s2); if (t2 != null) wimToken = t2;
            result.Logs.Add($"[리코추가] Step2 응답: {s2.Length}자");

            // Step 3
            result.Logs.Add("[리코추가] Step3: 비밀번호 설정...");
            var s3 = await PostFormAsync(client, $"{wb}/commitChPassword.cgi",
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
                $"{wb}/chPasswordPage.cgi");
            var t3 = ExtractWimToken(s3); if (t3 != null) wimToken = t3;
            result.Logs.Add($"[리코추가] Step3 응답: {s3.Length}자");

            // Step 4
            result.Logs.Add("[리코추가] Step4: 속성 확인...");
            var s4 = await PostFormAsync(client, $"{wb}/folderPropPage.cgi",
                new() {
                    ["wimToken"] = wimToken,
                    ["id"] = "", ["jt"] = "", ["el"] = "",
                    ["urlLang"] = "ko", ["urlProfile"] = profile,
                    ["pdfThumbnailURI"] = "", ["thumbnailURI"] = "", ["WidthSize"] = "",
                    ["subdocCount"] = "", ["targetDocId"] = folderId,
                    ["title"] = "", ["creator"] = "",
                    ["useInputParam"] = "false", ["useSavedParam"] = "",
                    ["subReturnDsp"] = "3", ["mode"] = "CREATE", ["wayTo"] = "",
                    ["selectedFolderId"] = folderId, ["useSavedPropParam"] = "true",
                    ["ID"] = "", ["simpleErrorMessage"] = "", ["dummy"] = "",
                },
                $"{wb}/commitChPassword.cgi");
            var t4 = ExtractWimToken(s4); if (t4 != null) wimToken = t4;
            result.Logs.Add($"[리코추가] Step4 응답: {s4.Length}자");

            // Step 5
            result.Logs.Add("[리코추가] Step5: 생성 확정...");
            var html = await PostFormAsync(client, $"{wb}/putFolderProp.cgi",
                new() {
                    ["wimToken"] = wimToken, ["targetFolderId"] = folderId,
                    ["changedFolderName"] = box.Name, ["mode"] = "CREATE",
                    ["targetDocId"] = "", ["selectedFolderId"] = "",
                    ["title"] = "", ["useSavedPropParam"] = "true",
                    ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
                },
                $"{wb}/folderPropPage.cgi");
            result.Logs.Add($"[리코추가] Step5 응답: {html.Length}자");

            var errMatch = Regex.Match(html, @"simpleErrorMessage[^>]*value=[""']([^""']+)");
            if (errMatch.Success && !string.IsNullOrWhiteSpace(errMatch.Groups[1].Value))
            {
                result.Logs.Add($"[리코추가][FAIL] 서버 에러: {errMatch.Groups[1].Value}");
                return DriverResult.Fail($"생성 실패: {errMatch.Groups[1].Value}", result.Logs);
            }

            var (_, _, afterFolders) = await ListFoldersAsync(client, baseUrl, device.Ip, result.Logs);
            var created = afterFolders.Any(f => f.name == box.Name);
            if (!created)
            {
                result.Logs.Add($"[리코추가][FAIL] 서버에 폴더 '{box.Name}' 미생성 — Step 응답 요약");
                result.Logs.Add($"  └ Step5 앞 400자: {html[..Math.Min(400, html.Length)]}");
                return DriverResult.Fail("생성 검증 실패 (서버에 폴더가 추가되지 않음)", result.Logs);
            }

            result.Logs.Add($"[리코추가] 생성 완료! ID={folderId}");
            result.Success = true;
            result.Message = "스캔함 추가 완료";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[리코추가][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"추가 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // DeleteScanBoxAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try
        {
            result.Logs.Add($"[리코삭제] 스캔함 삭제: {box.Name}");

            string? wimToken; string baseUrl; List<string> loginLogs;
            (client, wimToken, baseUrl, loginLogs) = await LoginAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null || wimToken == null)
                return DriverResult.Fail("로그인 실패", result.Logs);

            var (profile, newToken, folders) = await ListFoldersAsync(client, baseUrl, device.Ip, result.Logs);
            if (newToken != null) wimToken = newToken;

            var target = folders.FirstOrDefault(f => f.name == box.Name);
            if (target == default)
            {
                result.Logs.Add($"[리코삭제] 폴더 '{box.Name}' 찾을 수 없음 (이미 삭제됨)");
                result.Success = true;
                result.Message = "삭제 완료 (이미 없음)";
                return result;
            }

            result.Logs.Add($"[리코삭제] 대상 폴더 ID={target.id}, profile={profile}");

            var now = DateTime.Now;
            var pw = box.Password ?? "";
            var wb = $"{baseUrl}/web/{profile}/ko/webdocbox";
            string html;

            result.Logs.Add("[리코삭제] Step1: 삭제 페이지 진입...");
            html = await PostFormAsync(client, $"{wb}/folderDeletePage.cgi",
                new() {
                    ["wimToken"] = wimToken,
                    ["mode"] = "", ["selectedDocIds"] = "",
                    ["subReturnDsp"] = "", ["useInputParam"] = "",
                    ["useSavedPropParam"] = "false",
                    ["_hour"] = now.ToString("HH"), ["_min"] = now.ToString("mm"),
                    ["selectedFolderId"] = target.id,
                },
                $"{wb}/folderListPage.cgi");
            var t = ExtractWimToken(html); if (t != null) wimToken = t;
            result.Logs.Add($"[리코삭제] Step1 응답: {html.Length}자");

            if (!string.IsNullOrEmpty(pw))
            {
                result.Logs.Add("[리코삭제] Step2: 비밀번호 인증...");
                html = await PostFormAsync(client, $"{wb}/chPasswordPage.cgi",
                    new() {
                        ["wimToken"] = wimToken, ["mode"] = "AUTHENTICATE",
                        ["targetFolderId"] = target.id, ["wayTo"] = "DELETEFOLDER",
                        ["targetDocId"] = target.id, ["useSavedPropParam"] = "true",
                        ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
                    },
                    $"{wb}/folderDeletePage.cgi");
                t = ExtractWimToken(html); if (t != null) wimToken = t;

                result.Logs.Add("[리코삭제] Step3: 비밀번호 확정...");
                html = await PostFormAsync(client, $"{wb}/commitChPassword.cgi",
                    new() {
                        ["wimToken"] = wimToken, ["title"] = "", ["creator"] = "",
                        ["dataFormat"] = "", ["allPages"] = "false",
                        ["cid"] = "", ["convBW"] = "", ["backUp"] = "",
                        ["backUpFormatStr"] = "", ["backUpResoStr"] = "",
                        ["targetDocId"] = target.id, ["oldPassword"] = B64(pw),
                        ["newPassword"] = "undefined", ["confirmation"] = "undefined",
                        ["useInputParam"] = "false", ["useSavedParam"] = "",
                        ["subReturnDsp"] = "3", ["mode"] = "AUTHENTICATE",
                        ["wayTo"] = "DELETEFOLDER", ["useSavedPropParam"] = "false",
                        ["selectedFolderId"] = "", ["ID"] = "", ["dummy"] = "",
                    },
                    $"{wb}/chPasswordPage.cgi");
                t = ExtractWimToken(html); if (t != null) wimToken = t;

                result.Logs.Add("[리코삭제] Step4: 삭제 페이지 재진입...");
                html = await PostFormAsync(client, $"{wb}/folderDeletePage.cgi",
                    new() {
                        ["wimToken"] = wimToken,
                        ["id"] = "", ["jt"] = "", ["el"] = "",
                        ["urlLang"] = "ko", ["urlProfile"] = profile,
                        ["pdfThumbnailURI"] = "", ["thumbnailURI"] = "", ["WidthSize"] = "",
                        ["subdocCount"] = "", ["targetDocId"] = target.id,
                        ["title"] = "", ["creator"] = "",
                        ["useInputParam"] = "false", ["useSavedParam"] = "",
                        ["subReturnDsp"] = "3", ["mode"] = "AUTHENTICATE",
                        ["wayTo"] = "DELETEFOLDER", ["selectedFolderId"] = target.id,
                        ["useSavedPropParam"] = "false",
                        ["ID"] = "", ["simpleErrorMessage"] = "", ["dummy"] = "",
                    },
                    $"{wb}/commitChPassword.cgi");
                t = ExtractWimToken(html); if (t != null) wimToken = t;
            }

            result.Logs.Add("[리코삭제] 최종 삭제...");
            html = await PostFormAsync(client, $"{wb}/deleteDocContentsPage.cgi",
                new() {
                    ["wimToken"] = wimToken,
                    ["selectedDocIds"] = target.id,
                    ["subReturnDsp"] = "3",
                },
                $"{wb}/folderDeletePage.cgi");

            result.Logs.Add($"[리코삭제] 최종 응답: {html.Length}자");

            if (html.Length == 0)
            {
                result.Logs.Add("[리코삭제][FAIL] 빈 응답");
                return DriverResult.Fail("삭제 실패: 서버 응답 없음", result.Logs);
            }

            result.Logs.Add("[리코삭제] 삭제 완료");
            result.Success = true;
            result.Message = "삭제 완료";
            return result;
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[리코삭제][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"삭제 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    // ──────────────────────────────────────────────
    // UpdateScanBoxAsync
    // ──────────────────────────────────────────────

    public override async Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null)
    {
        var result = new DriverResult();
        HttpClient? client = null;
        try
        {
            var searchName = oldName ?? box.Name;
            result.Logs.Add($"[리코수정] 스캔함 수정: {searchName} → {box.Name}");

            string? wimToken; string baseUrl; List<string> loginLogs;
            (client, wimToken, baseUrl, loginLogs) = await LoginAsync(device);
            result.Logs.AddRange(loginLogs);
            if (client == null || wimToken == null)
                return DriverResult.Fail("로그인 실패", result.Logs);

            var (profile, newToken, folders) = await ListFoldersAsync(client, baseUrl, device.Ip, result.Logs);
            if (newToken != null) wimToken = newToken;

            var target = folders.FirstOrDefault(f => f.name == searchName);
            if (target == default)
                return DriverResult.Fail($"폴더 '{searchName}'을 찾을 수 없습니다.", result.Logs);

            result.Logs.Add($"[리코수정] 대상 폴더 ID={target.id}, profile={profile}");

            var now = DateTime.Now;
            var pw = box.Password ?? "";
            var changePw = !string.IsNullOrEmpty(pw);
            var wb = $"{baseUrl}/web/{profile}/ko/webdocbox";
            string html;

            result.Logs.Add("[리코수정] Step1: 속성 페이지 진입...");
            html = await PostFormAsync(client, $"{wb}/folderPropPage.cgi",
                new() {
                    ["wimToken"] = wimToken, ["mode"] = "PROPERTY",
                    ["selectedDocIds"] = "", ["subReturnDsp"] = "",
                    ["useInputParam"] = "", ["useSavedPropParam"] = "false",
                    ["_hour"] = now.ToString("HH"), ["_min"] = now.ToString("mm"),
                    ["selectedFolderId"] = target.id,
                },
                $"{wb}/folderListPage.cgi");
            var t = ExtractWimToken(html); if (t != null) wimToken = t;
            result.Logs.Add($"[리코수정] Step1 응답: {html.Length}자");

            result.Logs.Add("[리코수정] Step2: 비밀번호 페이지...");
            html = await PostFormAsync(client, $"{wb}/chPasswordPage.cgi",
                new() {
                    ["wimToken"] = wimToken, ["targetFolderId"] = target.id,
                    ["changedFolderName"] = box.Name, ["mode"] = "PROPERTY",
                    ["targetDocId"] = target.id, ["selectedFolderId"] = "",
                    ["title"] = box.Name, ["useSavedPropParam"] = "true",
                    ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
                },
                $"{wb}/folderPropPage.cgi");
            t = ExtractWimToken(html); if (t != null) wimToken = t;
            result.Logs.Add($"[리코수정] Step2 응답: {html.Length}자");

            var pwB64 = changePw ? B64(pw) : "";
            result.Logs.Add("[리코수정] Step3: 비밀번호 저장...");
            html = await PostFormAsync(client, $"{wb}/commitChPassword.cgi",
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
                $"{wb}/chPasswordPage.cgi");
            t = ExtractWimToken(html); if (t != null) wimToken = t;
            result.Logs.Add($"[리코수정] Step3 응답: {html.Length}자");

            result.Logs.Add("[리코수정] Step4: 수정 확정...");
            html = await PostFormAsync(client, $"{wb}/putFolderProp.cgi",
                new() {
                    ["wimToken"] = wimToken, ["targetFolderId"] = target.id,
                    ["changedFolderName"] = box.Name, ["mode"] = "PROPERTY",
                    ["targetDocId"] = "", ["selectedFolderId"] = "",
                    ["title"] = "", ["useSavedPropParam"] = "true",
                    ["useInputParam"] = "false", ["subReturnDsp"] = "3", ["dummy"] = "",
                },
                $"{wb}/folderPropPage.cgi");
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
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[리코수정][ERROR] {ex.Message}");
            result.Success = false;
            result.Message = $"수정 오류: {ex.Message}";
            return result;
        }
        finally { client?.Dispose(); }
    }

    /// <summary>리코 파일 목록 — 추후 구현</summary>
    public override Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box)
    {
        return Task.FromResult(DriverResult<List<BoxFile>>.Ok(new List<BoxFile>(), "리코 파일 목록 미구현"));
    }
}
