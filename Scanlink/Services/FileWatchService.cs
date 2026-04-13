using System.IO;
using Scanlink.Core;
using Scanlink.Drivers;
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
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var devices = _deviceService.Devices.ToList();
            var boxes = _scanBoxService.ScanBoxes.ToList();
            var totalBoxes = boxes.Count;
            var failedCount = 0;

            // 기기별 병렬 실행 (같은 기기 내 박스는 순차 — 세션 공유)
            var deviceTasks = devices.Select(async device =>
            {
                var driver = DriverFactory.GetDriver(device.Brand);
                if (driver == null) return 0;

                var deviceBoxes = boxes.Where(b => b.MfpDeviceId == device.Id).ToList();
                var localFailed = 0;
                foreach (var box in deviceBoxes)
                {
                    if (!await CheckBoxAsync(driver, device, box))
                        localFailed++;
                }
                return localFailed;
            }).ToList();

            var results = await Task.WhenAll(deviceTasks);
            failedCount = results.Sum();

            sw.Stop();
            AppLogger.Log("FileWatch",
                $"조회 완료: 기기 {devices.Count}대, 박스 {totalBoxes}개, 실패 {failedCount}건, 소요 {sw.ElapsedMilliseconds}ms");
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
        var tag = $"{device.DisplayName}/{box.Name}";
        try
        {
            var result = await driver.GetBoxFilesAsync(device, box);

            if (!result.Success || result.Data == null)
            {
                AppLogger.Log("FileWatch", $"[{tag}] 조회 실패: {result.Message}");
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

            AppLogger.Log("FileWatch", $"[{tag}] 조회 성공: 현재 {current.Count}개, 이전 {previous.Count}개");
            // 드라이버 내부 로그도 항상 출력 (상세 디버깅)
            foreach (var line in result.Logs)
                AppLogger.Log("FileWatch", $"  └ {line}");

            if (added.Count > 0 || removed.Count > 0)
            {
                AppLogger.Log("FileWatch", $"[{tag}] 차이 발견: 신규 {added.Count}, 삭제 {removed.Count}");

                if (added.Count > 0)
                {
                    var addedInfo = current.Where(f => added.Contains(f.DocId))
                        .Select(f => $"{f.Name}({f.DocId})");
                    AppLogger.Log("FileWatch", $"  + 신규: {string.Join(", ", addedInfo)}");

                    // 신규 파일 자동 다운로드 — 현재 신도만 지원
                    if (driver is SindohDriver sindoh)
                    {
                        var newFiles = current.Where(f => added.Contains(f.DocId)).ToList();
                        await DownloadSindohFilesAsync(sindoh, device, box, newFiles);
                    }
                }
                if (removed.Count > 0)
                {
                    var removedInfo = previous.Where(f => removed.Contains(f.DocId))
                        .Select(f => $"{f.Name}({f.DocId})");
                    AppLogger.Log("FileWatch", $"  - 삭제: {string.Join(", ", removedInfo)}");
                }
            }

            // 현재 상태 저장 (변경 유무와 관계없이)
            FileListStore.Save(device.Id, box.Id, current);
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.Error("FileWatch", $"[{tag}] 조회 예외", ex);
            return false;
        }
    }

    /// <summary>신도 신규 파일을 스캔함 로컬 경로에 자동 다운로드</summary>
    private async Task DownloadSindohFilesAsync(SindohDriver sindoh, MfpDevice device, ScanBox box, List<BoxFile> newFiles)
    {
        var tag = $"{device.DisplayName}/{box.Name}";

        if (string.IsNullOrEmpty(box.LocalFolder))
        {
            AppLogger.Log("FileWatch", $"[{tag}] 로컬 경로 미설정 — 다운로드 스킵");
            return;
        }

        try
        {
            if (!Directory.Exists(box.LocalFolder))
                Directory.CreateDirectory(box.LocalFolder);
        }
        catch (Exception ex)
        {
            AppLogger.Error("FileWatch", $"로컬 폴더 생성 실패: {box.LocalFolder}", ex);
            return;
        }

        foreach (var file in newFiles)
        {
            try
            {
                AppLogger.Log("FileWatch", $"[신도 {tag}] 다운로드 시작: {file.Name}");
                var dlResult = await sindoh.DownloadFileAsync(device, box, file);

                // 드라이버 로그는 성공/실패 관계없이 항상 출력
                foreach (var line in dlResult.Logs)
                    AppLogger.Log("FileWatch", $"  └ {line}");

                if (!dlResult.Success || dlResult.Data == null)
                {
                    AppLogger.Log("FileWatch", $"[신도 {tag}] 다운로드 실패: {dlResult.Message}");
                    continue;
                }

                var localPath = Path.Combine(box.LocalFolder, $"{file.Name}.pdf");
                await File.WriteAllBytesAsync(localPath, dlResult.Data);
                AppLogger.Log("FileWatch", $"[신도 {tag}] 저장 완료: {localPath} ({dlResult.Data.Length} bytes)");
            }
            catch (Exception ex)
            {
                AppLogger.Error("FileWatch", $"[신도 {tag}] 다운로드/저장 예외: {file.Name}", ex);
            }
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
        _lock.Dispose();
    }
}
