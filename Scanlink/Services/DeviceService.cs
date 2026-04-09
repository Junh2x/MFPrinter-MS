using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Services;

/// <summary>
/// 복합기 관리 서비스. JSON 파일로 영속 저장.
/// </summary>
public class DeviceService
{
    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Scanlink", "devices.json");

    public ObservableCollection<MfpDevice> Devices { get; } = new();

    public DeviceService()
    {
        Load();
    }

    public void AddDevice(MfpDevice device)
    {
        Devices.Add(device);
        Save();
    }

    public void RemoveDevice(MfpDevice device)
    {
        Devices.Remove(device);
        Save();
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SavePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(Devices.ToList(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, json);
        }
        catch (Exception ex)
        {
            AppLogger.Error("DeviceService", $"저장 실패: {ex.Message}");
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(SavePath)) return;

            var json = File.ReadAllText(SavePath);
            var devices = JsonSerializer.Deserialize<List<MfpDevice>>(json);
            if (devices == null) return;

            foreach (var d in devices)
                Devices.Add(d);

            AppLogger.Log("DeviceService", $"기기 {devices.Count}대 로드");
        }
        catch (Exception ex)
        {
            AppLogger.Error("DeviceService", $"로드 실패: {ex.Message}");
        }
    }
}
