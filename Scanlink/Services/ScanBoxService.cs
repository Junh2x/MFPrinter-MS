using System.Collections.ObjectModel;
using Scanlink.Models;

namespace Scanlink.Services;

/// <summary>
/// 스캔함 관리 서비스.
/// 실제 기능은 추후 구현 — 지금은 UI 연동용 스텁.
/// </summary>
public class ScanBoxService
{
    public ObservableCollection<ScanBox> ScanBoxes { get; } = new();

    public void AddScanBox(ScanBox box)
    {
        ScanBoxes.Add(box);
    }

    public void RemoveScanBox(ScanBox box)
    {
        ScanBoxes.Remove(box);
    }

    public void UpdateScanBox(ScanBox box)
    {
        // TODO: 복합기 동기화 + 로컬 DB 업데이트
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
}
