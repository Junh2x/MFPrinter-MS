using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;

namespace Scanlink.Services;

/// <summary>
/// 백그라운드에서 일정 간격으로 모든 기기의 모든 스캔함 내 파일 목록을
/// 조회하고, 이전에 저장된 목록과 비교하여 차이가 있으면 로그로 출력.
/// 파일 목록은 %LocalAppData%\Scanlink\filelists\ 에 영속 저장.
/// </summary>
public class FileWatchService : IDisposable
{
    private readonly DeviceService _deviceService;
    private readonly ScanBoxService _scanBoxService;
    private readonly System.Threading.Timer _timer;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public TimeSpan Interval { get; }

    public FileWatchService(DeviceService deviceService, ScanBoxService scanBoxService, TimeSpan? interval = null)
    {
        _deviceService = deviceService;
        _scanBoxService = scanBoxService;
        Interval = interval ?? TimeSpan.FromSeconds(10);
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
                foreach (var line in result.Logs)
                    AppLogger.Log("FileWatch", $"  └ {line}");
                return false;
            }

            var current = result.Data;
            var previous = FileListStore.Load(device.Id, box.Id);

            var currentIds = current.Select(f => f.DocId).ToHashSet();
            var previousIds = previous.Select(f => f.DocId).ToHashSet();

            var added = currentIds.Except(previousIds).ToList();
            var removed = previousIds.Except(currentIds).ToList();

            if (added.Count > 0 || removed.Count > 0)
            {
                AppLogger.Log("FileWatch",
                    $"[{device.DisplayName}/{box.Name}] 차이 발견: 신규 {added.Count}, 삭제 {removed.Count}");

                if (added.Count > 0)
                {
                    var addedInfo = current
                        .Where(f => added.Contains(f.DocId))
                        .Select(f => $"{f.Name}({f.DocId})");
                    AppLogger.Log("FileWatch",
                        $"  + 신규: {string.Join(", ", addedInfo)}");
                }
                if (removed.Count > 0)
                {
                    var removedInfo = previous
                        .Where(f => removed.Contains(f.DocId))
                        .Select(f => $"{f.Name}({f.DocId})");
                    AppLogger.Log("FileWatch",
                        $"  - 삭제: {string.Join(", ", removedInfo)}");
                }

                FileListStore.Save(device.Id, box.Id, current);
            }
            else if (previous.Count == 0 && current.Count > 0)
            {
                // 최초 저장
                AppLogger.Log("FileWatch",
                    $"[{device.DisplayName}/{box.Name}] 초기 파일 {current.Count}개 기록");
                FileListStore.Save(device.Id, box.Id, current);
            }
            else if (previous.Count == 0 && current.Count == 0)
            {
                // 빈 상태 유지 — 저장 파일이 없으면 빈 목록이라도 생성
                FileListStore.Save(device.Id, box.Id, current);
            }

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
