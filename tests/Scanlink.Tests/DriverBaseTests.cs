using Scanlink.Drivers.Canon;
using Scanlink.Drivers.Ricoh;
using Scanlink.Drivers.Sindoh;
using Scanlink.Models;
using Xunit;

namespace Scanlink.Tests;

/// <summary>
/// 각 Default 드라이버의 순수 로직 계약 테스트 (네트워크 X).
/// - Brand 속성 일치
/// - SetupAsync 기본 동작 (IsConfigured=true, 성공 반환)
/// - DisposeSessions 예외 없음, 반복 호출 안전
/// - 지원 안하는 기능의 기본 실패 반환
/// </summary>
public class DriverBaseTests
{
    [Fact]
    public void Canon_Has_Correct_Brand()
    {
        Assert.Equal(MfpBrand.Canon, new CanonDefaultDriver().Brand);
    }

    [Fact]
    public void Ricoh_Has_Correct_Brand()
    {
        Assert.Equal(MfpBrand.Ricoh, new RicohDefaultDriver().Brand);
    }

    [Fact]
    public void Sindoh_Has_Correct_Brand()
    {
        Assert.Equal(MfpBrand.Sindoh, new SindohDefaultDriver().Brand);
    }

    [Fact]
    public async Task Canon_SetupAsync_Marks_Configured_And_Returns_Ok()
    {
        var device = new MfpDevice { Brand = MfpBrand.Canon };
        var driver = new CanonDefaultDriver();

        var result = await driver.SetupAsync(device);

        Assert.True(result.Success);
        Assert.True(device.IsConfigured);
    }

    [Fact]
    public async Task Ricoh_SetupAsync_Marks_Configured_And_Returns_Ok()
    {
        var device = new MfpDevice { Brand = MfpBrand.Ricoh };
        var driver = new RicohDefaultDriver();

        var result = await driver.SetupAsync(device);

        Assert.True(result.Success);
        Assert.True(device.IsConfigured);
    }

    [Fact]
    public async Task Sindoh_SetupAsync_Marks_Configured_And_Returns_Ok()
    {
        var device = new MfpDevice { Brand = MfpBrand.Sindoh };
        var driver = new SindohDefaultDriver();

        var result = await driver.SetupAsync(device);

        Assert.True(result.Success);
        Assert.True(device.IsConfigured);
    }

    [Fact]
    public void DisposeSessions_Is_Safe_On_Fresh_Driver()
    {
        // 세션이 하나도 없는 상태에서 정리해도 예외 없어야 함
        new CanonDefaultDriver().DisposeSessions();
        new RicohDefaultDriver().DisposeSessions();
        new SindohDefaultDriver().DisposeSessions();
    }

    [Fact]
    public void DisposeSessions_Is_Idempotent()
    {
        // 연속 호출 안전
        var canon = new CanonDefaultDriver();
        canon.DisposeSessions();
        canon.DisposeSessions();
        canon.DisposeSessions();
    }

    [Fact]
    public async Task Canon_DownloadFileAsync_Returns_Unsupported_By_Default()
    {
        // 캐논은 페이지 단위 다운로드만 지원. 파일 단위는 기본 실패.
        var driver = new CanonDefaultDriver();
        var device = new MfpDevice { Brand = MfpBrand.Canon };
        var box = new ScanBox();
        var file = new BoxFile { DocId = "0001", Name = "test" };

        var result = await driver.DownloadFileAsync(device, box, file);

        Assert.False(result.Success);
        Assert.Contains("지원", result.Message);
    }

    [Fact]
    public async Task Ricoh_DownloadFileAsync_Returns_Unsupported_By_Default()
    {
        var driver = new RicohDefaultDriver();
        var device = new MfpDevice { Brand = MfpBrand.Ricoh };
        var box = new ScanBox();
        var file = new BoxFile { DocId = "001", Name = "test" };

        var result = await driver.DownloadFileAsync(device, box, file);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Ricoh_GetBoxFilesAsync_Returns_Empty_Ok()
    {
        // 리코 파일 목록은 미구현 — Ok + 빈 리스트 반환
        var driver = new RicohDefaultDriver();
        var device = new MfpDevice { Brand = MfpBrand.Ricoh };
        var box = new ScanBox();

        var result = await driver.GetBoxFilesAsync(device, box);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data!);
    }
}
