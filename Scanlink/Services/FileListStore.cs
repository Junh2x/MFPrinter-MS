using System.IO;
using System.Text.Json;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Services;

/// <summary>
/// 스캔함별 파일 목록 스냅샷을 JSON으로 영속 저장.
/// 경로: %LocalAppData%\Scanlink\filelists\{deviceId}_{boxId}.json
/// </summary>
public static class FileListStore
{
    private static readonly string BaseDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Scanlink", "filelists");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static string GetPath(string deviceId, string boxId) =>
        Path.Combine(BaseDir, $"{deviceId}_{boxId}.json");

    public static List<BoxFile> Load(string deviceId, string boxId)
    {
        var path = GetPath(deviceId, boxId);
        if (!File.Exists(path)) return [];
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<BoxFile>>(json) ?? [];
        }
        catch (Exception ex)
        {
            AppLogger.Error("FileListStore", $"로드 실패 ({deviceId}/{boxId})", ex);
            return [];
        }
    }

    public static void Save(string deviceId, string boxId, List<BoxFile> files)
    {
        try
        {
            if (!Directory.Exists(BaseDir))
                Directory.CreateDirectory(BaseDir);
            var path = GetPath(deviceId, boxId);
            var json = JsonSerializer.Serialize(files, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            AppLogger.Error("FileListStore", $"저장 실패 ({deviceId}/{boxId})", ex);
        }
    }
}
