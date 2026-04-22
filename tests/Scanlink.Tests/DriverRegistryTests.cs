using Scanlink.Core;
using Scanlink.Models;
using Xunit;

namespace Scanlink.Tests;

[Collection("RegistrySequential")]
public class DriverRegistryTests : IDisposable
{
    public DriverRegistryTests() => DriverRegistry.Clear();
    public void Dispose() => DriverRegistry.Clear();

    [Fact]
    public void Resolve_Returns_Null_When_No_Mapping()
    {
        var device = new MfpDevice { Brand = MfpBrand.Canon, Model = "iR-ADV" };
        Assert.Null(DriverRegistry.Resolve(device));
    }

    [Fact]
    public void Resolve_Returns_Brand_Default_When_No_Model_Match()
    {
        var canon = new FakeDriver(MfpBrand.Canon, "default");
        DriverRegistry.RegisterDefault(MfpBrand.Canon, canon);

        var device = new MfpDevice { Brand = MfpBrand.Canon, Model = "AnyModel" };
        Assert.Same(canon, DriverRegistry.Resolve(device));
    }

    [Fact]
    public void Resolve_Model_Match_Takes_Priority_Over_Default()
    {
        var def = new FakeDriver(MfpBrand.Sindoh, "default");
        var d430 = new FakeDriver(MfpBrand.Ricoh, "D430-as-ricoh");
        DriverRegistry.RegisterDefault(MfpBrand.Sindoh, def);
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D430", d430);

        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D430" };
        Assert.Same(d430, DriverRegistry.Resolve(device));
    }

    [Fact]
    public void Resolve_Model_Pattern_Is_Case_Insensitive()
    {
        var d430 = new FakeDriver(MfpBrand.Sindoh, "d430");
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D430", d430);

        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "d430" };
        Assert.Same(d430, DriverRegistry.Resolve(device));
    }

    [Fact]
    public void Resolve_Model_Pattern_Respects_Brand_Boundary()
    {
        // 같은 모델명 패턴이 다른 브랜드에 있더라도 브랜드가 다르면 매칭 안됨
        var sindohD430 = new FakeDriver(MfpBrand.Sindoh, "sindoh-d430");
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D430", sindohD430);

        var device = new MfpDevice { Brand = MfpBrand.Canon, Model = "D430" };
        Assert.Null(DriverRegistry.Resolve(device));
    }

    [Fact]
    public void Resolve_Empty_Model_Falls_Back_To_Default()
    {
        var def = new FakeDriver(MfpBrand.Sindoh, "default");
        var d430 = new FakeDriver(MfpBrand.Ricoh, "d430");
        DriverRegistry.RegisterDefault(MfpBrand.Sindoh, def);
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D430", d430);

        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "" };
        Assert.Same(def, DriverRegistry.Resolve(device));
    }

    [Fact]
    public void AllRegistered_Deduplicates_Shared_Instance()
    {
        // 같은 인스턴스를 기본 + 모델 양쪽에 등록
        var ricoh = new FakeDriver(MfpBrand.Ricoh, "shared");
        DriverRegistry.RegisterDefault(MfpBrand.Ricoh, ricoh);
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D430", ricoh);

        var all = DriverRegistry.AllRegistered();
        Assert.Single(all);
        Assert.Same(ricoh, all[0]);
    }

    [Fact]
    public void AllRegistered_Lists_Distinct_Instances()
    {
        var a = new FakeDriver(MfpBrand.Canon, "a");
        var b = new FakeDriver(MfpBrand.Ricoh, "b");
        var c = new FakeDriver(MfpBrand.Sindoh, "c");
        DriverRegistry.RegisterDefault(MfpBrand.Canon, a);
        DriverRegistry.RegisterDefault(MfpBrand.Ricoh, b);
        DriverRegistry.RegisterDefault(MfpBrand.Sindoh, c);

        var all = DriverRegistry.AllRegistered();
        Assert.Equal(3, all.Count);
        Assert.Contains(a, all);
        Assert.Contains(b, all);
        Assert.Contains(c, all);
    }

    [Fact]
    public void DisposeAllSessions_Calls_Each_Instance_Exactly_Once()
    {
        var shared = new FakeDriver(MfpBrand.Ricoh, "shared");
        var other = new FakeDriver(MfpBrand.Canon, "other");
        DriverRegistry.RegisterDefault(MfpBrand.Ricoh, shared);
        DriverRegistry.RegisterDefault(MfpBrand.Canon, other);
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D430", shared);  // shared 3번 등록
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D431", shared);

        DriverRegistry.DisposeAllSessions();

        Assert.Equal(1, shared.DisposeCallCount);  // 3회 등록이지만 1회만 호출
        Assert.Equal(1, other.DisposeCallCount);
    }

    [Fact]
    public void DisposeAllSessions_Swallows_Exceptions()
    {
        var throwing = new ThrowingDriver();
        var normal = new FakeDriver(MfpBrand.Canon);
        DriverRegistry.RegisterDefault(MfpBrand.Ricoh, throwing);
        DriverRegistry.RegisterDefault(MfpBrand.Canon, normal);

        // 예외가 나도 다음 드라이버 정리가 진행되어야 함
        DriverRegistry.DisposeAllSessions();

        Assert.Equal(1, normal.DisposeCallCount);
    }

    [Fact]
    public void RegisterModel_Multiple_Patterns_Registration_Order_Wins()
    {
        var first = new FakeDriver(MfpBrand.Sindoh, "first");
        var second = new FakeDriver(MfpBrand.Sindoh, "second");
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D4\d\d", first);
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D430", second);  // 둘 다 매칭되지만 first가 먼저 등록됨

        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D430" };
        Assert.Same(first, DriverRegistry.Resolve(device));
    }

    [Fact]
    public void Resolve_Null_Or_Empty_Model_With_Model_Rule_Skips_Rule()
    {
        // Model이 비어있으면 모델 규칙을 건너뛰고 기본으로 가야 함
        var def = new FakeDriver(MfpBrand.Sindoh, "default");
        var d430 = new FakeDriver(MfpBrand.Ricoh, "d430");
        DriverRegistry.RegisterDefault(MfpBrand.Sindoh, def);
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"^$", d430);  // 빈 문자열 매칭 정규식

        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "" };
        // 구현상: Model이 빈 문자열이면 foreach 진입 자체가 스킵됨 → default 반환
        Assert.Same(def, DriverRegistry.Resolve(device));
    }

    private sealed class ThrowingDriver : IMfpDriver
    {
        public MfpBrand Brand => MfpBrand.Ricoh;
        public void DisposeSessions() => throw new InvalidOperationException("정리 실패");
        public Task<DriverResult> ConnectAsync(MfpDevice device) => Task.FromResult(DriverResult.Ok());
        public Task<DriverResult> SetupAsync(MfpDevice device) => Task.FromResult(DriverResult.Ok());
        public Task<DriverResult<List<ScanBox>>> GetScanBoxListAsync(MfpDevice device) => Task.FromResult(DriverResult<List<ScanBox>>.Ok(new()));
        public Task<DriverResult> AddScanBoxAsync(MfpDevice device, ScanBox box) => Task.FromResult(DriverResult.Ok());
        public Task<DriverResult> UpdateScanBoxAsync(MfpDevice device, ScanBox box, string? oldName = null, string? oldPassword = null) => Task.FromResult(DriverResult.Ok());
        public Task<DriverResult> DeleteScanBoxAsync(MfpDevice device, ScanBox box) => Task.FromResult(DriverResult.Ok());
        public Task<DriverResult<List<BoxFile>>> GetBoxFilesAsync(MfpDevice device, ScanBox box) => Task.FromResult(DriverResult<List<BoxFile>>.Ok(new()));
        public Task<DriverResult<byte[]>> DownloadFileAsync(MfpDevice device, ScanBox box, BoxFile file) => Task.FromResult(DriverResult<byte[]>.Ok(Array.Empty<byte>()));
    }
}
