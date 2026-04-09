using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Scanlink.Models;

namespace Scanlink.Services;

/// <summary>
/// 복합기 검색 및 브랜드 식별 서비스.
/// HTTP 접속으로 브랜드/모델을 식별하고, 서브넷 스캔으로 기기를 탐색한다.
/// </summary>
public class DeviceDiscoveryService
{
    private static readonly int[] HttpPorts = [80, 8000, 443, 8443];
    private static readonly int[] PrinterPorts = [9100, 631, 80, 8000, 443];
    private static readonly TimeSpan TcpTimeout = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(4);

    private static readonly Dictionary<MfpBrand, string[]> BrandKeywords = new()
    {
        [MfpBrand.Canon] = ["canon", "imagerunner", "ir-adv", "iradv", "remote ui", "meap"],
        [MfpBrand.Ricoh] = ["ricoh", "im c", "imc", "web image monitor", "savin", "lanier"],
        [MfpBrand.Sindoh] = ["sindoh", "신도", "d420", "d450", "d-color", "n410", "n610"],
    };

    private static readonly Dictionary<MfpBrand, string> ModelPatterns = new()
    {
        [MfpBrand.Canon] = @"((?:iR[ -]?ADV|imageRUNNER)\s*\S+(?:\s+\S+)?)",
        [MfpBrand.Ricoh] = @"((?:IM\s*C|MP\s*C?|SP)\s*\d+\S*)",
        [MfpBrand.Sindoh] = @"([DNdn][ -]?\d{3,4}\S*)",
    };

    /// <summary>단일 IP에 대해 브랜드/모델 식별</summary>
    public async Task<MfpDevice?> IdentifyDeviceAsync(string ip)
    {
        Debug.WriteLine($"[검색] {ip} 식별 시작");

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };
        using var client = new HttpClient(handler) { Timeout = HttpTimeout };
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        foreach (var port in HttpPorts)
        {
            var scheme = port is 443 or 8443 ? "https" : "http";
            var url = $"{scheme}://{ip}:{port}/";

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) continue;

                var html = await response.Content.ReadAsStringAsync();
                var server = response.Headers.Server?.ToString() ?? "";
                var allText = (html + " " + server).ToLower();

                var brand = MatchBrand(allText);
                if (brand == MfpBrand.Unknown) continue;

                var model = ExtractModel(html, brand);
                var baseUrl = port is 80 or 443 ? $"{scheme}://{ip}" : $"{scheme}://{ip}:{port}";

                Debug.WriteLine($"[검색] {ip} → {brand} {model} (port={port})");

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
            catch
            {
                // 다음 포트 시도
            }
        }

        Debug.WriteLine($"[검색] {ip} → 브랜드 식별 실패");
        return null;
    }

    /// <summary>서브넷 자동 검색</summary>
    public async Task<List<MfpDevice>> ScanSubnetAsync(
        string? subnetPrefix = null,
        Action<int, int>? progressCallback = null,
        CancellationToken ct = default)
    {
        subnetPrefix ??= GetLocalSubnet();
        if (subnetPrefix == null)
        {
            Debug.WriteLine("[검색] 로컬 서브넷 감지 실패");
            return [];
        }

        Debug.WriteLine($"[검색] 서브넷 스캔 시작: {subnetPrefix}.0/24");

        // 1단계: 프린터 포트 열린 IP 찾기
        var openIps = new List<string>();
        var scanned = 0;
        var tasks = new List<Task>();

        var semaphore = new SemaphoreSlim(50);
        for (var i = 1; i < 255; i++)
        {
            if (ct.IsCancellationRequested) break;

            var ip = $"{subnetPrefix}.{i}";
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    if (await HasPrinterPortAsync(ip))
                    {
                        lock (openIps) openIps.Add(ip);
                        Debug.WriteLine($"[검색] 포트 열림: {ip}");
                    }
                }
                finally
                {
                    semaphore.Release();
                    var count = Interlocked.Increment(ref scanned);
                    progressCallback?.Invoke(count, 254);
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
        Debug.WriteLine($"[검색] 포트 스캔 완료: {openIps.Count}개 발견");

        // 2단계: HTTP로 브랜드 식별
        var devices = new List<MfpDevice>();
        for (var i = 0; i < openIps.Count; i++)
        {
            if (ct.IsCancellationRequested) break;

            var device = await IdentifyDeviceAsync(openIps[i]);
            if (device != null)
                devices.Add(device);
        }

        Debug.WriteLine($"[검색] 식별 완료: {devices.Count}대");
        return devices;
    }

    private static async Task<bool> HasPrinterPortAsync(string ip)
    {
        foreach (var port in PrinterPorts)
        {
            try
            {
                using var tcp = new TcpClient();
                var connectTask = tcp.ConnectAsync(ip, port);
                if (await Task.WhenAny(connectTask, Task.Delay(TcpTimeout)) == connectTask
                    && tcp.Connected)
                    return true;
            }
            catch
            {
                // 다음 포트
            }
        }
        return false;
    }

    private static MfpBrand MatchBrand(string text)
    {
        foreach (var (brand, keywords) in BrandKeywords)
            foreach (var kw in keywords)
                if (text.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return brand;
        return MfpBrand.Unknown;
    }

    private static string ExtractModel(string html, MfpBrand brand)
    {
        if (!ModelPatterns.TryGetValue(brand, out var pattern)) return "";
        var m = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
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
        catch
        {
            return null;
        }
    }
}
