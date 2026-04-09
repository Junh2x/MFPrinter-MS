using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Services;

/// <summary>
/// 스캔함 관리 서비스. JSON 파일로 영속 저장.
/// </summary>
public class ScanBoxService
{
    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Scanlink", "scanboxes.json");

    public ObservableCollection<ScanBox> ScanBoxes { get; } = new();

    public ScanBoxService()
    {
        Load();
    }

    public void AddScanBox(ScanBox box)
    {
        ScanBoxes.Add(box);
        Save();
    }

    public void RemoveScanBox(ScanBox box)
    {
        ScanBoxes.Remove(box);
        Save();
    }

    public void UpdateScanBox(ScanBox box)
    {
        Save();
    }

    public ObservableCollection<ScanBox> GetScanBoxesForDevice(string deviceId)
    {
        var filtered = new ObservableCollection<ScanBox>();
        foreach (var box in ScanBoxes)
        {
            if (box.MfpDeviceId == deviceId)
                filtered.Add(box);
        }
        return filtered;
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SavePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(ScanBoxes.ToList(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, json);
        }
        catch (Exception ex)
        {
            AppLogger.Error("ScanBoxService", $"저장 실패: {ex.Message}");
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(SavePath)) return;

            var json = File.ReadAllText(SavePath);
            var boxes = JsonSerializer.Deserialize<List<ScanBox>>(json);
            if (boxes == null) return;

            foreach (var box in boxes)
                ScanBoxes.Add(box);

            AppLogger.Log("ScanBoxService", $"스캔함 {boxes.Count}개 로드");
        }
        catch (Exception ex)
        {
            AppLogger.Error("ScanBoxService", $"로드 실패: {ex.Message}");
        }
    }
}
