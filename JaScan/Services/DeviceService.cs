using System.Collections.ObjectModel;
using JaScan.Models;

namespace JaScan.Services;

/// <summary>
/// 복합기 관리 서비스 (검색, 추가, 삭제).
/// 실제 기능은 추후 구현 — 지금은 UI 연동용 스텁.
/// </summary>
public class DeviceService
{
    public ObservableCollection<MfpDevice> Devices { get; } = new();

    public void AddDevice(MfpDevice device)
    {
        Devices.Add(device);
    }

    public void RemoveDevice(MfpDevice device)
    {
        Devices.Remove(device);
    }

    // TODO: 실제 네트워크 검색 구현
    public async Task<List<MfpDevice>> SearchDevicesAsync()
    {
        await Task.Delay(100);
        return new List<MfpDevice>();
    }
}
