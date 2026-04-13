using System.Collections.Concurrent;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Services;

/// <summary>
/// 백그라운드에서 일정 간격으로 모든 기기의 모든 스캔함 내 파일 목록을
/// 조회하고 변경 사항을 로그에 기록.
/// </summary>
public class FileWatchService : IDisposable
{
    private readonly DeviceService _deviceService;
    private readonly ScanBoxService _scanBoxService;
    private readonly System.Threading.Timer _timer;
    private readonly SemaphoreSlim _lock = new(1, 1);

    // 이전 조회 상태: (deviceId, boxId) → DocId 집합
    private readonly ConcurrentDictionary<string, HashSet<string>> _snapshot = new();

    public TimeSpan Interval { get; }

    public FileWatchService(DeviceService deviceService, ScanBoxService scanBoxService, TimeSpan? interval = null)
    {
        _deviceService = deviceService;
        _scanBoxService = scanBoxService;
        Interval = interval ?? TimeSpan.FromSeconds(5);
        _timer = new System.Threading.Timer(async _ => await TickAsync(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void Start()
    {
        AppLogger.Log("FileWatch", $"파일 감시 시작 (주기: {Interval.TotalSeconds}초)");
        _timer.Change(TimeSpan.Zero, Interval);
    }

    public void Stop()
    {
        AppLogger.Log("FileWatch", "파일 감시 중지");
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    private async Task TickAsync()
    {
        // 중복 실행 방지 (이전 주기가 아직 끝나지 않았으면 건너뜀)
        if (!await _lock.WaitAsync(0)) return;
        try
        {
            var devices = _deviceService.Devices.ToList();
            var boxes = _scanBoxService.ScanBoxes.ToList();
            var totalBoxes = 0;
            var failed = 0;

            foreach (var device in devices)
            {
                var driver = DriverFactory.GetDriver(device.Brand);
                if (driver == null) continue;

                var deviceBoxes = boxes.Where(b => b.MfpDeviceId == device.Id).ToList();
                totalBoxes += deviceBoxes.Count;
                foreach (var box in deviceBoxes)
                {
                    if (!await CheckBoxAsync(driver, device, box))
                        failed++;
                }
            }

            AppLogger.Log("FileWatch", $"조회 완료: 기기 {devices.Count}대, 박스 {totalBoxes}개, 실패 {failed}건");
        }
        catch (Exception ex)
        {
            AppLogger.Error("FileWatch", "Tick 오류", ex);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<bool> CheckBoxAsync(IMfpDriver driver, MfpDevice device, ScanBox box)
    {
        try
        {
            var result = await driver.GetBoxFilesAsync(device, box);
            if (!result.Success || result.Data == null)
            {
                AppLogger.Log("FileWatch",
                    $"[{device.DisplayName}/{box.Name}] 조회 실패: {result.Message}");
                // 상세 내부 로그도 전부 출력
                foreach (var line in result.Logs)
                    AppLogger.Log("FileWatch", $"  └ {line}");
                return false;
            }

            var currentIds = result.Data.Select(f => f.DocId).ToHashSet();
            var key = $"{device.Id}/{box.Id}";

            if (!_snapshot.TryGetValue(key, out var prevIds))
            {
                _snapshot[key] = currentIds;
                AppLogger.Log("FileWatch",
                    $"[{device.DisplayName}/{box.Name}] 초기 파일 {currentIds.Count}개");
                return true;
            }

            var added = currentIds.Except(prevIds).ToList();
            var removed = prevIds.Except(currentIds).ToList();

            if (added.Count > 0)
            {
                var addedNames = result.Data
                    .Where(f => added.Contains(f.DocId))
                    .Select(f => $"{f.Name}({f.DocId})");
                AppLogger.Log("FileWatch",
                    $"[{device.DisplayName}/{box.Name}] 신규 파일 {added.Count}개: {string.Join(", ", addedNames)}");
            }

            if (removed.Count > 0)
            {
                AppLogger.Log("FileWatch",
                    $"[{device.DisplayName}/{box.Name}] 삭제된 파일 {removed.Count}개: {string.Join(", ", removed)}");
            }

            _snapshot[key] = currentIds;
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.Error("FileWatch", $"[{device.DisplayName}/{box.Name}] 조회 실패", ex);
            return false;
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
        _lock.Dispose();
    }
}
