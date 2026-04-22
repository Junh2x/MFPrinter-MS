using Scanlink.Core;
using Scanlink.Models;
using Xunit;

namespace Scanlink.Tests;

/// <summary>
/// 테스트용 가짜 드라이버. 호출 횟수 추적 + 임의 Brand 지정.
/// </summary>
internal sealed class FakeDriver : IMfpDriver
{
    public MfpBrand Brand { get; }
    public string Label { get; }
    public int DisposeCallCount { get; private set; }

    public FakeDriver(MfpBrand brand, string label = "")
    {
        Brand = brand;
        Label = label;
    }

    public void DisposeSessions() => DisposeCallCount++;

    public Task<DriverResult> ConnectAsync(MfpDevice device) => Task.FromResult(DriverResult.Ok());
    public Task<DriverResult> SetupAsync(MfpDevice device) { device.IsConfigured = true; return Task.FromResult(DriverResult.Ok()); }
    public Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device) => Task.FromResult(DriverResult<List<ScanBox>>.Ok(new()));
    public Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box) => Task.FromResult(DriverResult.Ok());
    public Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null) => Task.FromResult(DriverResult.Ok());
    public Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box) => Task.FromResult(DriverResult.Ok());
    public Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box) => Task.FromResult(DriverResult<List<BoxFile>>.Ok(new()));
    public Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file) => Task.FromResult(DriverResult<byte[]>.Ok(Array.Empty<byte>()));
}

/// <summary>
/// 테스트 시 DriverRegistry 상태를 복원하기 위한 fixture.
/// 테스트 끝날 때마다 레지스트리를 초기화.
/// </summary>
public sealed class RegistryResetFixture : IDisposable
{
    public RegistryResetFixture() => DriverRegistry.Clear();
    public void Dispose() => DriverRegistry.Clear();
}

[CollectionDefinition("RegistrySequential", DisableParallelization = true)]
public class RegistrySequentialCollection { }
