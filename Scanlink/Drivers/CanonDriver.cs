using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Scanlink.Core;
using Scanlink.Models;

namespace Scanlink.Drivers;

/// <summary>
/// 캐논 복합기 드라이버.
/// 고급박스 설정, 주소록(원터치) CRUD, SMB 폴더 생성을 담당한다.
/// </summary>
public class CanonDriver : IMfpDriver
{
    public MfpBrand Brand => MfpBrand.Canon;

    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

    private static string Dummy() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

    // ──────────────────────────────────────────────
    // 세션 헬퍼
    // ──────────────────────────────────────────────

    private static (HttpClient client, CookieContainer cookies) CreateClient()
    {
        var cookies = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            UseCookies = true,
        };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        client.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9");
        return (client, cookies);
    }

    /// <summary>포털 접속 → nativetop → iR 쿠키 획득</summary>
    private static async Task<(HttpClient? client, List<string> logs)> InitSessionAsync(string baseUrl)
    {
        var logs = new List<string>();
        var (client, cookies) = CreateClient();

        // Step 1: 포털
        logs.Add($"[세션] 포털 접속: {baseUrl}/");
        var r = await client.GetAsync($"{baseUrl}/");
        if (!r.IsSuccessStatusCode)
        {
            logs.Add($"[세션][FAIL] 포털 접속 실패: HTTP {(int)r.StatusCode}");
            return (null, logs);
        }
        logs.Add($"[세션] 포털 OK — 쿠키: {cookies.Count}개");

        await Task.Delay(300);

        // Step 2: nativetop → iR 쿠키
        logs.Add("[세션] nativetop 요청 (iR 쿠키 획득)...");
        var nativeUrl = $"{baseUrl}/rps/nativetop.cgi?RUIPNxBundle=&CorePGTAG=PGTAG_ADR_USR&Dummy={Dummy()}";
        var req = new HttpRequestMessage(HttpMethod.Get, nativeUrl);
        req.Headers.Add("Referer", $"{baseUrl}/");
        await client.SendAsync(req);

        var allCookies = cookies.GetCookies(new Uri(baseUrl));
        var hasIR = allCookies.Cast<Cookie>().Any(c => c.Name == "iR");
        if (!hasIR)
        {
            logs.Add("[세션][FAIL] iR 쿠키 미발급");
            return (null, logs);
        }
        logs.Add("[세션] iR 쿠키 획득 완료");

        return (client, logs);
    }

    // ──────────────────────────────────────────────
    // 토큰/파싱 헬퍼
    // ──────────────────────────────────────────────

    private static string? ExtractTokenFromUrl(string html)
    {
        var m = Regex.Match(html, @"Token=(\d+)");
        return m.Success ? m.Groups[1].Value : null;
    }

    private static string? ExtractTokenFromHidden(string html)
    {
        var m = Regex.Match(html, @"name=[""']Token[""'][^>]*value=[""']([^""']+)");
        return m.Success ? m.Groups[1].Value : null;
    }

    /// <summary>BookNameList 파싱 → {id: name}</summary>
    private static Dictionary<int, string> ParseBookNameList(string html)
    {
        var books = new Dictionary<int, string>();
        var match = Regex.Match(html, @"BookNameList\s*=\s*\{(.*?)\}", RegexOptions.Singleline);
        if (!match.Success) return books;

        foreach (Match m in Regex.Matches(match.Groups[1].Value, @"(\d+)\s*:\s*""([^""]*)"""))
            books[int.Parse(m.Groups[1].Value)] = m.Groups[2].Value.Trim();

        return books;
    }

    /// <summary>adrsList 파싱 → {idx: {tp, nm, ad, ot}}</summary>
    private static Dictionary<int, Dictionary<string, string>> ParseAddrsList(string html)
    {
        var entries = new Dictionary<int, Dictionary<string, string>>();
        var match = Regex.Match(html, @"var\s+adrsList\s*=\s*\{(.*?)\};", RegexOptions.Singleline);
        if (!match.Success) return entries;

        foreach (Match m in Regex.Matches(match.Groups[1].Value, @"(\d+)\s*:\s*\{([^}]*)\}"))
        {
            var idx = int.Parse(m.Groups[1].Value);
            var props = new Dictionary<string, string>();
            foreach (Match pm in Regex.Matches(m.Groups[2].Value, @"(\w+)\s*:\s*(?:(\d+)|""([^""]*)"")"))
            {
                var key = pm.Groups[1].Value;
                var val = pm.Groups[2].Success ? pm.Groups[2].Value : pm.Groups[3].Value.Trim();
                props[key] = val;
            }
            entries[idx] = props;
        }
        return entries;
    }

    /// <summary>사용 중인 슬롯 번호 집합</summary>
    private static HashSet<int> GetUsedSlots(string html)
    {
        var used = new HashSet<int>();
        foreach (Match m in Regex.Matches(html, @"\{[^}]*idx:(\d+)[^}]*nm:""([^""]*)"""))
        {
            if (!string.IsNullOrWhiteSpace(m.Groups[2].Value))
                used.Add(int.Parse(m.Groups[1].Value));
        }
        return used;
    }

    /// <summary>빈 슬롯 번호 중 가장 작은 것 반환</summary>
    private static int FindEmptySlot(HashSet<int> used, int maxSlot = 200)
    {
        for (var i = 1; i <= maxSlot; i++)
            if (!used.Contains(i))
                return i;
        return -1;
    }

    private static async Task<string> PostFormAsync(HttpClient client, string url, string body, string referer)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        req.Headers.Add("Referer", referer);
        var r = await client.SendAsync(req);
        return await r.Content.ReadAsStringAsync();
    }

    // ──────────────────────────────────────────────
    // ConnectAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult> ConnectAsync(MfpDevice device)
    {
        var result = new DriverResult();
        var baseUrl = $"http://{device.Ip}:{device.Port}";

        var (client, logs) = await InitSessionAsync(baseUrl);
        result.Logs.AddRange(logs);

        if (client == null)
        {
            result.Success = false;
            result.Message = "연결 실패";
            return result;
        }

        device.BaseUrl = baseUrl;
        device.Status = ConnectionStatus.Connected;
        result.Success = true;
        result.Message = "연결 성공";
        result.Logs.Add($"[연결] {device.Ip}:{device.Port} 연결 성공");
        return result;
    }

    // ──────────────────────────────────────────────
    // SetupAsync — 고급박스 설정 + 원터치 AID 검색
    // ──────────────────────────────────────────────

    public async Task<DriverResult> SetupAsync(MfpDevice device)
    {
        var result = new DriverResult();
        var baseUrl = device.BaseUrl;

        result.Logs.Add($"[설정] 캐논 초기 설정 시작: {device.Ip}");

        // 세션 초기화
        var (client, sessionLogs) = await InitSessionAsync(baseUrl);
        result.Logs.AddRange(sessionLogs);
        if (client == null) return DriverResult.Fail("세션 초기화 실패", result.Logs);

        // ── 고급박스 설정 ──
        result.Logs.Add("[설정] 고급박스 설정 페이지 접근...");
        var settingsUrl = $"{baseUrl}/rps/cdsuperbox.cgi?Flag=Init_Data&PageFlag=c_superbox.tpl&FuncTypeFlag=SettingPage&Dummy={Dummy()}";
        var html = await (await client.GetAsync(settingsUrl)).Content.ReadAsStringAsync();
        var token = ExtractTokenFromHidden(html);
        if (token == null)
        {
            result.Logs.Add("[설정][FAIL] 고급박스 Token 추출 실패");
            return DriverResult.Fail("고급박스 Token 추출 실패", result.Logs);
        }
        result.Logs.Add($"[설정] Token 획득: {token[..Math.Min(15, token.Length)]}...");

        var payload = new Dictionary<string, string>
        {
            ["OpenOutSide"] = "1",
            ["PermitMakeDir"] = "1",
            ["ReadOnlyMode"] = "0",
            ["PermitManage"] = "0",
            ["PermitFileType"] = "0",
            ["OperationLogValid"] = "1",
            ["Setting_SMB"] = "",
            ["Setting_WebDAV"] = "",
            ["Setting_DOCLIB"] = "1",
            ["WebDav_AuthType"] = "0",
            ["WebDav_UseSSL"] = "1",
            ["AutoDelete"] = "1",
            ["AutoDeleteTime_HH"] = "00",
            ["AutoDeleteTime_MM"] = "00",
            ["Flag"] = "Exec_Data",
            ["PageFlag"] = "c_sboxlist.tpl",
            ["FuncTypeFlag"] = "SettingPage",
            ["CoreNXAction"] = "./cdsuperbox.cgi",
            ["CoreNXPage"] = "c_superbox.tpl",
            ["disp"] = "",
            ["Dummy"] = Dummy(),
            ["Token"] = token,
        };

        result.Logs.Add("[설정] 고급박스 설정 POST...");
        var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/rps/cdsuperbox.cgi")
        {
            Content = new FormUrlEncodedContent(payload)
        };
        req.Headers.Add("Referer", settingsUrl);
        var resp = await client.SendAsync(req);
        var respText = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode || respText.Contains("ERR"))
        {
            result.Logs.Add($"[설정][FAIL] 고급박스 설정 실패: HTTP {(int)resp.StatusCode}");
            return DriverResult.Fail("고급박스 설정 실패", result.Logs);
        }
        result.Logs.Add("[설정] 고급박스 설정 완료");

        await Task.Delay(500);

        // ── 원터치 AID 검색 ──
        result.Logs.Add("[설정] 주소록 목록 조회 (원터치 AID 검색)...");
        html = await (await client.GetAsync(
            $"{baseUrl}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&Dummy={Dummy()}")).Content.ReadAsStringAsync();

        var books = ParseBookNameList(html);
        result.Logs.Add($"[설정] 주소록 {books.Count}개 발견:");
        foreach (var (id, name) in books)
            result.Logs.Add($"  [{id}] {name}");

        var oneTouchEntry = books.FirstOrDefault(b =>
            b.Value.Contains("원터치", StringComparison.OrdinalIgnoreCase));

        if (oneTouchEntry.Value != null)
        {
            device.AddressBookId = oneTouchEntry.Key.ToString();
            result.Logs.Add($"[설정] 원터치 AID 발견: {device.AddressBookId} ({oneTouchEntry.Value})");
        }
        else
        {
            result.Logs.Add("[설정][WARN] '원터치' 주소록을 찾지 못함. 첫 번째 주소록 사용.");
            if (books.Count > 0)
            {
                var first = books.First();
                device.AddressBookId = first.Key.ToString();
                result.Logs.Add($"[설정] 대체 AID: {device.AddressBookId} ({first.Value})");
            }
            else
            {
                result.Logs.Add("[설정][FAIL] 주소록이 없습니다.");
                return DriverResult.Fail("주소록을 찾을 수 없습니다.", result.Logs);
            }
        }

        device.IsConfigured = true;
        result.Success = true;
        result.Message = "초기 설정 완료";
        result.Logs.Add($"[설정] 캐논 초기 설정 완료 (AID={device.AddressBookId})");
        return result;
    }

    // ──────────────────────────────────────────────
    // GetScanBoxListAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device)
    {
        var result = new DriverResult<List<ScanBox>> { Logs = [] };
        var baseUrl = device.BaseUrl;
        var aid = device.AddressBookId;

        result.Logs.Add($"[조회] 수신지 목록 조회: AID={aid}");

        var (client, sessionLogs) = await InitSessionAsync(baseUrl);
        result.Logs.AddRange(sessionLogs);
        if (client == null) return DriverResult<List<ScanBox>>.Fail("세션 초기화 실패", result.Logs);

        // asublist
        await client.GetAsync($"{baseUrl}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&Dummy={Dummy()}");
        await Task.Delay(300);

        // alframe + albody
        await client.GetAsync($"{baseUrl}/rps/alframe.cgi?AID={aid}");
        await Task.Delay(300);

        var html = await PostFormAsync(client,
            $"{baseUrl}/rps/albody.cgi",
            $"AID={aid}&FILTER_ID=0&Dummy={Dummy()}",
            $"{baseUrl}/rps/alframe.cgi?");

        var entries = ParseAddrsList(html);
        var boxes = new List<ScanBox>();

        foreach (var (idx, props) in entries)
        {
            var nm = props.GetValueOrDefault("nm", "");
            if (string.IsNullOrWhiteSpace(nm)) continue;

            boxes.Add(new ScanBox
            {
                Name = nm,
                SlotIndex = idx,
                MfpDeviceId = device.Id,
            });
            result.Logs.Add($"  [{idx}] {nm}");
        }

        result.Logs.Add($"[조회] {boxes.Count}개 수신지 발견");
        result.Success = true;
        result.Data = boxes;
        result.Message = $"{boxes.Count}개 조회";
        return result;
    }

    // ──────────────────────────────────────────────
    // AddScanBoxAsync — SMB 폴더 생성 + 주소록 등록
    // ──────────────────────────────────────────────

    public async Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        var baseUrl = device.BaseUrl;
        var aid = device.AddressBookId;
        var folderPath = $@"\share\folder\{box.Name}";

        result.Logs.Add($"[추가] 스캔함 추가 시작: {box.Name}");
        result.Logs.Add($"  기기: {device.Ip}:{device.Port}, AID={aid}");
        result.Logs.Add($"  폴더: {folderPath}");

        // ── 1. SMB 폴더 생성 ──
        var uncPath = $@"\\{device.Ip}\share\folder\{box.Name}";
        result.Logs.Add($"[추가] SMB 폴더 생성: {uncPath}");
        try
        {
            if (!Directory.Exists(uncPath))
            {
                Directory.CreateDirectory(uncPath);
                result.Logs.Add("[추가] SMB 폴더 생성 완료");
            }
            else
            {
                result.Logs.Add("[추가] SMB 폴더 이미 존재");
            }
        }
        catch (Exception ex)
        {
            result.Logs.Add($"[추가][FAIL] SMB 폴더 생성 실패: {ex.Message}");
            return DriverResult.Fail($"SMB 폴더 생성 실패: {ex.Message}", result.Logs);
        }

        // ── 2. 세션 초기화 ──
        var (client, sessionLogs) = await InitSessionAsync(baseUrl);
        result.Logs.AddRange(sessionLogs);
        if (client == null) return DriverResult.Fail("세션 초기화 실패", result.Logs);

        await Task.Delay(300);

        // ── 3. asublist → Token A ──
        result.Logs.Add("[추가] asublist → Token A 획득...");
        var html = await (await client.GetAsync(
            $"{baseUrl}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={Dummy()}"))
            .Content.ReadAsStringAsync();
        var tokenA = ExtractTokenFromUrl(html);
        if (tokenA == null)
        {
            result.Logs.Add("[추가][FAIL] Token A 추출 실패");
            return DriverResult.Fail("Token A 추출 실패", result.Logs);
        }
        result.Logs.Add($"[추가] Token A = {tokenA[..Math.Min(15, tokenA.Length)]}...");
        await Task.Delay(300);

        // ── 4. alframe → AID 컨텍스트 ──
        await client.GetAsync($"{baseUrl}/rps/alframe.cgi?AID={aid}");
        result.Logs.Add("[추가] alframe OK");
        await Task.Delay(300);

        // ── 5. albody → 빈 슬롯 찾기 ──
        html = await PostFormAsync(client,
            $"{baseUrl}/rps/albody.cgi",
            $"AID={aid}&FILTER_ID=0&Dummy={Dummy()}",
            $"{baseUrl}/rps/alframe.cgi?AID={aid}");

        var usedSlots = GetUsedSlots(html);
        var slot = FindEmptySlot(usedSlots);
        if (slot < 0)
        {
            result.Logs.Add("[추가][FAIL] 빈 슬롯 없음");
            return DriverResult.Fail("빈 슬롯이 없습니다.", result.Logs);
        }
        result.Logs.Add($"[추가] 빈 슬롯 선택: {slot} (사용중: {usedSlots.Count}개)");
        await Task.Delay(300);

        // ── 6. aprop (이메일 폼 ACLS=2) → Token B1 ──
        result.Logs.Add("[추가] aprop (ACLS=2 이메일 폼) → Token B1...");
        html = await PostFormAsync(client,
            $"{baseUrl}/rps/aprop.cgi?",
            $"AMOD=1&AID={aid}&AIDX={slot}&ACLS=2&AFION=1&AdrAction=.%2Falframe.cgi%3F&Dummy={Dummy()}&Token={tokenA}",
            $"{baseUrl}/rps/albody.cgi");
        var tokenB1 = ExtractTokenFromHidden(html);
        if (tokenB1 == null)
        {
            result.Logs.Add("[추가][FAIL] Token B1 추출 실패");
            return DriverResult.Fail("Token B1 추출 실패", result.Logs);
        }
        result.Logs.Add($"[추가] Token B1 = {tokenB1[..Math.Min(15, tokenB1.Length)]}...");
        await Task.Delay(300);

        // ── 7. aprop (파일 타입 변경 ACLS=7, Token 빈값) → Token B2 ──
        result.Logs.Add("[추가] aprop (ACLS=7 파일 타입) → Token B2...");
        html = await PostFormAsync(client,
            $"{baseUrl}/rps/aprop.cgi?",
            $"AID={aid}&PageFlag=&AIDX={slot}&ANAME=&ANAMEONE=&AREAD=&APNO=0&AAD1=" +
            $"&ACLS=7&DATADIV=&AdrAction=.%2Falframe.cgi%3F&AMOD=1" +
            $"&Dummy={Dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=&Token=",
            $"{baseUrl}/rps/aprop.cgi?");
        var tokenB2 = ExtractTokenFromHidden(html);
        if (tokenB2 == null)
        {
            result.Logs.Add("[추가][FAIL] Token B2 추출 실패");
            return DriverResult.Fail("Token B2 추출 실패", result.Logs);
        }
        result.Logs.Add($"[추가] Token B2 = {tokenB2[..Math.Min(15, tokenB2.Length)]}...");
        await Task.Delay(300);

        // ── 8. aprop (파일 설정 PageFlag=a_rfn_f.tpl, PASSCHK=1&빈값) → Token B3 ──
        var pw = box.Password ?? "";
        result.Logs.Add("[추가] aprop (파일 설정) → Token B3...");
        html = await PostFormAsync(client,
            $"{baseUrl}/rps/aprop.cgi",
            $"AID={aid}&PageFlag=a_rfn_f.tpl&AIDX={slot}" +
            $"&ANAME={Uri.EscapeDataString(box.Name)}&ANAMEONE={Uri.EscapeDataString(box.Name)}&AREAD={Uri.EscapeDataString(box.Name)}&APNO=0" +
            $"&AAD1={device.Ip}&ACLS=7&APRTCL=7" +
            $"&APATH={Uri.EscapeDataString(folderPath)}&AUSER=&INPUT_PSWD=0&APWORD={Uri.EscapeDataString(pw)}" +
            $"&PASSCHK=1&PASSCHK=" +
            $"&AdrAction=.%2Falframe.cgi%3F&AMOD=1" +
            $"&Dummy={Dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=" +
            $"&Token={tokenB2}",
            $"{baseUrl}/rps/aprop.cgi?");
        var tokenB3 = ExtractTokenFromHidden(html);
        if (tokenB3 == null)
        {
            result.Logs.Add("[추가][FAIL] Token B3 추출 실패");
            return DriverResult.Fail("Token B3 추출 실패", result.Logs);
        }
        result.Logs.Add($"[추가] Token B3 = {tokenB3[..Math.Min(15, tokenB3.Length)]}...");
        await Task.Delay(300);

        // ── 9. aprop (폴더 설정 PageFlag 없음) → Token B4 ──
        result.Logs.Add("[추가] aprop (폴더 설정) → Token B4...");
        html = await PostFormAsync(client,
            $"{baseUrl}/rps/aprop.cgi",
            $"AID={aid}&PageFlag=&AIDX={slot}" +
            $"&ANAME={Uri.EscapeDataString(box.Name)}&ANAMEONE={Uri.EscapeDataString(box.Name)}&AREAD={Uri.EscapeDataString(box.Name)}&APNO=0" +
            $"&AAD1={device.Ip}&ACLS=7&APRTCL=7" +
            $"&APATH={Uri.EscapeDataString(folderPath)}&AUSER=&INPUT_PSWD=0&APWORD={Uri.EscapeDataString(pw)}" +
            $"&PASSCHK=1&PASSCHK=" +
            $"&AdrAction=.%2Falframe.cgi%3F&AMOD=1" +
            $"&Dummy={Dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=" +
            $"&Token={tokenB3}",
            $"{baseUrl}/rps/aprop.cgi");
        var tokenB4 = ExtractTokenFromHidden(html);
        if (tokenB4 == null)
        {
            result.Logs.Add("[추가][FAIL] Token B4 추출 실패");
            return DriverResult.Fail("Token B4 추출 실패", result.Logs);
        }
        result.Logs.Add($"[추가] Token B4 = {tokenB4[..Math.Min(15, tokenB4.Length)]}...");
        await Task.Delay(300);

        // ── 10. anewadrs.cgi (최종 등록) ──
        result.Logs.Add("[추가] anewadrs.cgi (최종 등록)...");
        html = await PostFormAsync(client,
            $"{baseUrl}/rps/anewadrs.cgi",
            $"AID={aid}&PageFlag=&AIDX={slot}" +
            $"&ANAME={Uri.EscapeDataString(box.Name)}&ANAMEONE={Uri.EscapeDataString(box.Name)}&AREAD={Uri.EscapeDataString(box.Name)}&APNO=0" +
            $"&AAD1={device.Ip}&ACLS=7&APRTCL=7" +
            $"&APATH={Uri.EscapeDataString(folderPath)}&AUSER=&INPUT_PSWD=0&APWORD={Uri.EscapeDataString(pw)}" +
            $"&PASSCHK=1&PASSCHK=1" +
            $"&AdrAction=.%2Faprop.cgi%3F&AMOD=1" +
            $"&Dummy={Dummy()}&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=" +
            $"&Token={tokenB4}",
            $"{baseUrl}/rps/aprop.cgi");

        if (html.Contains("ERR_SUBMIT_FORM"))
        {
            result.Logs.Add("[추가][FAIL] 등록 실패 — ERR_SUBMIT_FORM");
            return DriverResult.Fail("주소록 등록 실패 (ERR_SUBMIT_FORM)", result.Logs);
        }

        box.SlotIndex = slot;
        result.Logs.Add($"[추가] 등록 완료! 슬롯={slot}, 이름={box.Name}");
        result.Success = true;
        result.Message = "스캔함 추가 완료";
        return result;
    }

    // ──────────────────────────────────────────────
    // DeleteScanBoxAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        var baseUrl = device.BaseUrl;
        var aid = device.AddressBookId;

        result.Logs.Add($"[삭제] 스캔함 삭제 시작: {box.Name} (슬롯={box.SlotIndex})");

        if (box.SlotIndex < 0)
        {
            result.Logs.Add("[삭제][FAIL] 슬롯 번호 없음");
            return DriverResult.Fail("슬롯 번호가 없습니다.", result.Logs);
        }

        var (client, sessionLogs) = await InitSessionAsync(baseUrl);
        result.Logs.AddRange(sessionLogs);
        if (client == null) return DriverResult.Fail("세션 초기화 실패", result.Logs);

        await Task.Delay(300);

        // asublist
        await client.GetAsync($"{baseUrl}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={Dummy()}");
        await Task.Delay(300);

        // alframe
        await client.GetAsync($"{baseUrl}/rps/alframe.cgi?AID={aid}");
        await Task.Delay(300);

        // albody → Token
        var html = await PostFormAsync(client,
            $"{baseUrl}/rps/albody.cgi",
            $"AID={aid}&FILTER_ID=0&Dummy={Dummy()}",
            $"{baseUrl}/rps/alframe.cgi?");
        var token = ExtractTokenFromHidden(html);
        if (token == null)
        {
            result.Logs.Add("[삭제][FAIL] Token 추출 실패");
            return DriverResult.Fail("삭제용 Token 추출 실패", result.Logs);
        }
        result.Logs.Add($"[삭제] Token = {token[..Math.Min(15, token.Length)]}...");
        await Task.Delay(300);

        // adelete.cgi
        result.Logs.Add("[삭제] adelete.cgi 요청...");
        html = await PostFormAsync(client,
            $"{baseUrl}/rps/adelete.cgi?",
            $"AMOD=0&AID={aid}&AIDX={box.SlotIndex}&ACLS=7&AFION=1" +
            $"&AdrAction=.%2Falframe.cgi%3F&Dummy={Dummy()}&Token={token}",
            $"{baseUrl}/rps/albody.cgi");

        if (html.Contains("ERR"))
        {
            result.Logs.Add("[삭제][FAIL] 삭제 응답에 ERR 포함");
            return DriverResult.Fail("삭제 실패", result.Logs);
        }

        result.Logs.Add($"[삭제] 삭제 완료! 슬롯={box.SlotIndex}");
        result.Success = true;
        result.Message = "삭제 완료";
        return result;
    }

    // ──────────────────────────────────────────────
    // UpdateScanBoxAsync
    // ──────────────────────────────────────────────

    public async Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box)
    {
        var result = new DriverResult();
        var baseUrl = device.BaseUrl;
        var aid = device.AddressBookId;
        var folderPath = $@"\share\folder\{box.Name}";

        result.Logs.Add($"[수정] 스캔함 수정 시작: {box.Name} (슬롯={box.SlotIndex})");

        if (box.SlotIndex < 0)
            return DriverResult.Fail("슬롯 번호가 없습니다.", result.Logs);

        var (client, sessionLogs) = await InitSessionAsync(baseUrl);
        result.Logs.AddRange(sessionLogs);
        if (client == null) return DriverResult.Fail("세션 초기화 실패", result.Logs);

        await Task.Delay(300);

        // Token A
        var html = await (await client.GetAsync(
            $"{baseUrl}/rps/asublist.cgi?CorePGTAG=24&AMOD=0&FromTopPage=1&Dummy={Dummy()}"))
            .Content.ReadAsStringAsync();
        var tokenA = ExtractTokenFromUrl(html);
        if (tokenA == null)
            return DriverResult.Fail("Token A 추출 실패", result.Logs);
        result.Logs.Add($"[수정] Token A = {tokenA[..Math.Min(15, tokenA.Length)]}...");
        await Task.Delay(300);

        // alframe + albody
        await client.GetAsync($"{baseUrl}/rps/alframe.cgi?AID={aid}");
        await Task.Delay(300);
        await PostFormAsync(client, $"{baseUrl}/rps/albody.cgi",
            $"AID={aid}&FILTER_ID=0&Dummy={Dummy()}", $"{baseUrl}/rps/alframe.cgi?");
        await Task.Delay(300);

        // aprop (수정 폼 AMOD=2) → Token B
        result.Logs.Add("[수정] aprop (AMOD=2 수정 폼) → Token B...");
        html = await PostFormAsync(client,
            $"{baseUrl}/rps/aprop.cgi?",
            $"AMOD=2&AID={aid}&AIDX={box.SlotIndex}&ACLS=7&AFION=1" +
            $"&AdrAction=.%2Falframe.cgi%3F&Dummy={Dummy()}&Token={tokenA}",
            $"{baseUrl}/rps/albody.cgi");
        var tokenB = ExtractTokenFromHidden(html);
        if (tokenB == null)
            return DriverResult.Fail("Token B 추출 실패 (수정 폼)", result.Logs);
        result.Logs.Add($"[수정] Token B = {tokenB[..Math.Min(15, tokenB.Length)]}...");
        await Task.Delay(300);

        // amodadrs.cgi (최종 수정)
        var pw = box.Password ?? "";
        var changePw = !string.IsNullOrEmpty(pw);
        result.Logs.Add($"[수정] amodadrs.cgi (비밀번호 변경={changePw})...");

        html = await PostFormAsync(client,
            $"{baseUrl}/rps/amodadrs.cgi",
            $"AID={aid}&PageFlag=&AIDX={box.SlotIndex}" +
            $"&ANAME={Uri.EscapeDataString(box.Name)}&AAD1={device.Ip}" +
            $"&APATH={Uri.EscapeDataString(folderPath)}&AUSER=" +
            $"&INPUT_PSWD=0&PASSCHK={(changePw ? "1" : "0")}&APWORD={Uri.EscapeDataString(pw)}" +
            $"&ACLS=7&APRTCL=7&APNO=0" +
            $"&AREAD={Uri.EscapeDataString(box.Name)}&ANAMEONE={Uri.EscapeDataString(box.Name)}" +
            $"&AMOD=0&Dummy={Dummy()}" +
            $"&AdrAction=.%2Falframe.cgi%3F" +
            $"&AFCLS=&AFINT=&APNOL=&AFION=1&AUUID=" +
            $"&Token={tokenB}",
            $"{baseUrl}/rps/aprop.cgi");

        if (html.Contains("ERR") && !html.Contains("ERR_SUBMIT_FORM"))
        {
            result.Logs.Add("[수정][FAIL] 수정 실패");
            return DriverResult.Fail("수정 실패", result.Logs);
        }

        result.Logs.Add("[수정] 수정 완료!");
        result.Success = true;
        result.Message = "수정 완료";
        return result;
    }
}
