using Scanlink.Drivers;
using Scanlink.Models;

namespace Scanlink.Core;

/// <summary>
/// 브랜드별 드라이버 인스턴스를 반환하는 팩토리.
/// </summary>
public static class DriverFactory
{
    private static readonly CanonDriver Canon = new();
    private static readonly RicohDriver Ricoh = new();
    private static readonly SindohDriver Sindoh = new();

    public static IMfpDriver? GetDriver(MfpBrand brand) => brand switch
    {
        MfpBrand.Canon => Canon,
        MfpBrand.Ricoh => Ricoh,
        MfpBrand.Sindoh => Sindoh,
        _ => null,
    };
}
