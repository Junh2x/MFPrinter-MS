using Scanlink.Core;
using Scanlink.Drivers.Canon;
using Scanlink.Drivers.Ricoh;
using Scanlink.Drivers.Sindoh;
using Scanlink.Models;
using Xunit;

namespace Scanlink.Tests;

/// <summary>
/// DriverFactory는 static ctor에서 전역 레지스트리를 채운다. 이 테스트는 DriverRegistryTests와
/// 충돌하지 않도록 별도 컬렉션에 두고, Factory를 한 번 touch한 뒤 결과만 검증.
/// Factory 접근 순서: RegistrySequential 컬렉션 안에서 Clear() 후 Factory를 건드리면 다시 등록되지 않음
/// (static ctor는 AppDomain 당 1회) — 따라서 이 테스트는 "첫 touch" 케이스와 충돌하므로
/// 별도 컬렉션으로 격리.
/// </summary>
[Collection("RegistrySequential")]
public class DriverFactoryTests : IDisposable
{
    public DriverFactoryTests() => DriverFactory.RegisterDefaults();
    public void Dispose() => DriverFactory.RegisterDefaults();

    [Fact]
    public void GetDriver_Returns_CanonDefault_For_Canon_Brand()
    {
        var device = new MfpDevice { Brand = MfpBrand.Canon, Model = "iR-ADV C3530" };
        var driver = DriverFactory.GetDriver(device);

        Assert.NotNull(driver);
        Assert.IsType<CanonDefaultDriver>(driver);
        Assert.Equal(MfpBrand.Canon, driver!.Brand);
    }

    [Fact]
    public void GetDriver_Returns_RicohDefault_For_Ricoh_Brand()
    {
        var device = new MfpDevice { Brand = MfpBrand.Ricoh, Model = "IM C2010" };
        var driver = DriverFactory.GetDriver(device);

        Assert.NotNull(driver);
        Assert.IsType<RicohDefaultDriver>(driver);
        Assert.Equal(MfpBrand.Ricoh, driver!.Brand);
    }

    [Fact]
    public void GetDriver_Returns_SindohDefault_For_Unmapped_Sindoh_Model()
    {
        // 사용자 요구: 매핑 안된 모델은 브랜드 기본값(현재 개발된 플로우) 사용
        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D450" };
        var driver = DriverFactory.GetDriver(device);

        Assert.NotNull(driver);
        Assert.IsType<SindohDefaultDriver>(driver);
    }

    [Fact]
    public void GetDriver_Sindoh_D430_Routes_To_RicohDefault()
    {
        // 신도 D430은 Ricoh WIM 펌웨어 기반 → Ricoh 드라이버로 라우팅
        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D430" };
        var driver = DriverFactory.GetDriver(device);

        Assert.NotNull(driver);
        Assert.IsType<RicohDefaultDriver>(driver);
    }

    [Fact]
    public void GetDriver_Sindoh_D430_Shares_Same_Instance_As_Ricoh_Default()
    {
        // 같은 RicohDefaultDriver 인스턴스가 재사용되는지 (메모리 절약 + 캐시 공유)
        var ricohDevice = new MfpDevice { Brand = MfpBrand.Ricoh, Model = "IM C2010" };
        var d430Device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D430" };

        var ricohDriver = DriverFactory.GetDriver(ricohDevice);
        var d430Driver = DriverFactory.GetDriver(d430Device);

        Assert.Same(ricohDriver, d430Driver);
    }

    [Fact]
    public void GetDriver_Sindoh_D420_Routes_To_SindohD420Driver()
    {
        // 신도 D420은 레거시 /wcd/user.cgi + HTML 인터페이스 → 전용 드라이버
        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D420" };
        var driver = DriverFactory.GetDriver(device);

        Assert.NotNull(driver);
        Assert.IsType<SindohD420Driver>(driver);
        Assert.Equal(MfpBrand.Sindoh, driver!.Brand);
    }

    [Fact]
    public void GetDriver_Sindoh_D420_NotSame_As_SindohDefault()
    {
        // D420은 기본 드라이버와 다른 전용 인스턴스여야 함
        var d420 = DriverFactory.GetDriver(new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D420" });
        var d450 = DriverFactory.GetDriver(new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D450" });

        Assert.NotSame(d420, d450);
    }

    [Fact]
    public void GetDriver_Returns_Null_For_Unknown_Brand()
    {
        var device = new MfpDevice { Brand = MfpBrand.Unknown, Model = "anything" };
        var driver = DriverFactory.GetDriver(device);

        Assert.Null(driver);
    }

    [Fact]
    public void GetDriver_Empty_Model_Returns_Brand_Default()
    {
        var device = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "" };
        var driver = DriverFactory.GetDriver(device);

        Assert.NotNull(driver);
        Assert.IsType<SindohDefaultDriver>(driver);
    }

    [Fact]
    public void GetDriver_Same_Brand_Returns_Same_Singleton_Instance()
    {
        // 같은 브랜드에 대해 여러 번 호출해도 항상 같은 드라이버 인스턴스 (정적 세션 캐시 공유)
        var dev1 = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D450" };
        var dev2 = new MfpDevice { Brand = MfpBrand.Sindoh, Model = "D999" };

        Assert.Same(DriverFactory.GetDriver(dev1), DriverFactory.GetDriver(dev2));
    }
}
