using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Services;

/// <summary>
/// 복합기 검색 및 브랜드 식별 서비스.
/// 1차: SNMP sysDescr (UDP 161) — 제조사/모델 정확히 식별
/// 2차: HTTP 포트 탐색 — 관리 URL 확인
/// </summary>
public class DeviceDiscoveryService
{
    private static readonly int[] HttpPorts = [80, 8000, 443, 8443];
    private static readonly TimeSpan SnmpTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan TcpTimeout = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(3);

    // SNMP v1 sysDescr OID: 1.3.6.1.2.1.1.1.0
    private static readonly byte[] SnmpOidSysDescr =
        [0x06, 0x08, 0x2b, 0x06, 0x01, 0x02, 0x01, 0x01, 0x01, 0x00];

    private static readonly (MfpBrand brand, string[] keywords)[] BrandRules =
    [
        (MfpBrand.Sindoh, ["sindoh", "신도리코"]),
        (MfpBrand.Canon,  ["canon"]),
        (MfpBrand.Ricoh,  ["ricoh"]),
    ];

    private static readonly Dictionary<MfpBrand, string> ModelPatterns = new()
    {
        [MfpBrand.Canon]  = @"((?:iR[ -]?ADV|imageRUNNER)\s*\S+(?:\s+\S+)?)",
        [MfpBrand.Ricoh]  = @"((?:IM\s*C|MP\s*C?|SP)\s*\d+\S*)",
        [MfpBrand.Sindoh] = @"([DNdn][ -]?\d{3,4}\S*)",
    };

    // ─────────────────────────────────────────
    // SNMP v1 GET sysDescr (raw UDP)
    // ─────────────────────────────────────────

    /// <summary>SNMP v1 GET sysDescr. 제조사/모델 문자열 반환.</summary>
    public static async Task<string?> SnmpGetSysDescrAsync(string ip, int timeoutMs = 2000)
    {
        // SNMP v1 GET Request 패킷 조립
        var community = "public"u8.ToArray();
        var varbind = new byte[] { 0x30 };
        var oidNull = new byte[SnmpOidSysDescr.Length + 2];
        Array.Copy(SnmpOidSysDescr, oidNull, SnmpOidSysDescr.Length);
        oidNull[^2] = 0x05; // NULL type
        oidNull[^1] = 0x00; // NULL length

        var varbindContent = new byte[] { 0x30, (byte)oidNull.Length };
        varbindContent = [.. varbindContent, .. oidNull];
        var varbindList = new byte[] { 0x30, (byte)varbindContent.Length };
        varbindList = [.. varbindList, .. varbindContent];

        var requestId  = new byte[] { 0x02, 0x01, 0x01 };
        var errorStat  = new byte[] { 0x02, 0x01, 0x00 };
        var errorIdx   = new byte[] { 0x02, 0x01, 0x00 };
        var pduBody = new byte[requestId.Length + errorStat.Length + errorIdx.Length + varbindList.Length];
        var offset = 0;
        Array.Copy(requestId, 0, pduBody, offset, requestId.Length); offset += requestId.Length;
        Array.Copy(errorStat, 0, pduBody, offset, errorStat.Length); offset += errorStat.Length;
        Array.Copy(errorIdx,  0, pduBody, offset, errorIdx.Length);  offset += errorIdx.Length;
        Array.Copy(varbindList, 0, pduBody, offset, varbindList.Length);

        var pdu = new byte[] { 0xa0, (byte)pduBody.Length };
        pdu = [.. pdu, .. pduBody];

        var version = new byte[] { 0x02, 0x01, 0x00 }; // v1
        var commTlv = new byte[] { 0x04, (byte)community.Length };
        commTlv = [.. commTlv, .. community];

        var msgBody = new byte[version.Length + commTlv.Length + pdu.Length];
        offset = 0;
        Array.Copy(version, 0, msgBody, offset, version.Length); offset += version.Length;
        Array.Copy(commTlv, 0, msgBody, offset, commTlv.Length); offset += commTlv.Length;
        Array.Copy(pdu, 0, msgBody, offset, pdu.Length);

        var message = new byte[] { 0x30, (byte)msgBody.Length };
        message = [.. message, .. msgBody];

        // UDP 전송
        using var udp = new UdpClient();
        udp.Client.ReceiveTimeout = timeoutMs;

        try
        {
            await udp.SendAsync(message, message.Length, new IPEndPoint(IPAddress.Parse(ip), 161));

            var receiveTask = udp.ReceiveAsync();
            if (await Task.WhenAny(receiveTask, Task.Delay(timeoutMs)) != receiveTask)
                return null;

            var data = receiveTask.Result.Buffer;

            // sysDescr OID 위치 찾기
            var idx = FindBytes(data, SnmpOidSysDescr);
            if (idx < 0) return null;

            idx += SnmpOidSysDescr.Length;
            if (idx >= data.Length || data[idx] != 0x04) return null; // OctetString type

            var strLen = data[idx + 1];
            var startIdx = idx + 2;

            // 긴 길이 처리 (length > 127)
            if ((strLen & 0x80) != 0)
            {
                var numBytes = strLen & 0x7F;
                strLen = 0;
                for (var i = 0; i < numBytes; i++)
                    strLen = (byte)((strLen << 8) | data[idx + 2 + i]);
                startIdx = idx + 2 + numBytes;
            }

            return Encoding.UTF8.GetString(data, startIdx, Math.Min(strLen, data.Length - startIdx));
        }
        catch
        {
            return null;
        }
    }

    private static int FindBytes(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            var found = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j]) { found = false; break; }
            }
            if (found) return i;
        }
        return -1;
    }

    // ─────────────────────────────────────────
    // HTTP 포트 탐색 (관리 URL 확인용)
    // ─────────────────────────────────────────

    private static async Task<(int port, string baseUrl)?> FindHttpPortAsync(string ip)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };
        using var client = new HttpClient(handler) { Timeout = HttpTimeout };
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

        foreach (var port in HttpPorts)
        {
            var scheme = port is 443 or 8443 ? "https" : "http";
            var url = $"{scheme}://{ip}:{port}/";
            try
            {
                var r = await client.GetAsync(url);
                if (r.IsSuccessStatusCode)
                {
                    var baseUrl = port is 80 or 443 ? $"{scheme}://{ip}" : $"{scheme}://{ip}:{port}";
                    return (port, baseUrl);
                }
            }
            catch { }
        }
        return null;
    }

    // ─────────────────────────────────────────
    // 단일 IP 식별
    // ─────────────────────────────────────────

    /// <summary>단일 IP에 대해 SNMP + HTTP로 브랜드/모델 식별</summary>
    public async Task<MfpDevice?> IdentifyDeviceAsync(string ip)
    {
        AppLogger.Log($"[식별] {ip} — SNMP 조회 시작");

        // 1. SNMP sysDescr
        var sysDescr = await SnmpGetSysDescrAsync(ip);

        if (sysDescr != null)
        {
            AppLogger.Log($"[식별] {ip} — SNMP sysDescr: {sysDescr}");

            var brand = MatchBrand(sysDescr);
            if (brand != MfpBrand.Unknown)
            {
                var model = ExtractModel(sysDescr, brand);
                AppLogger.Log($"[식별] {ip} — 브랜드: {brand}, 모델: {model}");

                // HTTP 포트 탐색 (관리 URL용)
                var httpInfo = await FindHttpPortAsync(ip);
                var port = httpInfo?.port ?? 80;
                var baseUrl = httpInfo?.baseUrl ?? $"http://{ip}";

                AppLogger.Log($"[식별] {ip} — HTTP 포트: {port}, URL: {baseUrl}");

                return new MfpDevice
                {
                    Ip = ip,
                    Brand = brand,
                    Model = model,
                    Port = port,
                    BaseUrl = baseUrl,
                    Status = ConnectionStatus.Connected,
                };
            }
            else
            {
                AppLogger.Log($"[식별] {ip} — SNMP 응답 있으나 지원 브랜드 아님: {sysDescr}");
            }
        }
        else
        {
            AppLogger.Log($"[식별] {ip} — SNMP 무응답");
        }

        return null;
    }

    // ─────────────────────────────────────────
    // 서브넷 자동 검색
    // ─────────────────────────────────────────

    /// <summary>서브넷 내 SNMP 응답하는 복합기 검색</summary>
    public async Task<List<MfpDevice>> ScanSubnetAsync(
        string? subnetPrefix = null,
        Action<int, int>? progressCallback = null,
        CancellationToken ct = default)
    {
        subnetPrefix ??= GetLocalSubnet();
        if (subnetPrefix == null)
        {
            AppLogger.Log("[검색] 로컬 서브넷 감지 실패");
            return [];
        }

        AppLogger.Log($"[검색] 서브넷 SNMP 스캔: {subnetPrefix}.0/24");

        // SNMP 병렬 스캔 (UDP이므로 매우 빠름)
        var devices = new List<MfpDevice>();
        var scanned = 0;
        var semaphore = new SemaphoreSlim(100);

        var tasks = Enumerable.Range(1, 254).Select(async i =>
        {
            if (ct.IsCancellationRequested) return;

            var ip = $"{subnetPrefix}.{i}";
            await semaphore.WaitAsync(ct);
            try
            {
                var device = await IdentifyDeviceAsync(ip);
                if (device != null)
                    lock (devices) devices.Add(device);
            }
            finally
            {
                semaphore.Release();
                var count = Interlocked.Increment(ref scanned);
                progressCallback?.Invoke(count, 254);
            }
        });

        await Task.WhenAll(tasks);

        devices.Sort((a, b) => string.Compare(a.Ip, b.Ip, StringComparison.Ordinal));
        AppLogger.Log($"[검색] 완료: {devices.Count}대 식별");
        return devices;
    }

    // ─────────────────────────────────────────
    // 유틸
    // ─────────────────────────────────────────

    private static MfpBrand MatchBrand(string text)
    {
        var lower = text.ToLower();
        foreach (var (brand, keywords) in BrandRules)
            foreach (var kw in keywords)
                if (lower.Contains(kw))
                    return brand;
        return MfpBrand.Unknown;
    }

    private static string ExtractModel(string text, MfpBrand brand)
    {
        if (!ModelPatterns.TryGetValue(brand, out var pattern)) return "";
        var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value.Trim() : "";
    }

    private static string? GetLocalSubnet()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect("8.8.8.8", 80);
            var localIp = (socket.LocalEndPoint as System.Net.IPEndPoint)?.Address.ToString();
            if (localIp == null) return null;
            var parts = localIp.Split('.');
            return $"{parts[0]}.{parts[1]}.{parts[2]}";
        }
        catch { return null; }
    }
}
