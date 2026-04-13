using Scanlink.Drivers;
using Scanlink.Models;

namespace Scanlink.Core;

/// <summary>
/// 기기별 드라이버를 반환하는 팩토리.
///
/// Brand(표시용 제조사)와 DriverKind(실제 API 구현)을 분리한다.
/// 대부분 Brand → 동일 DriverKind로 매핑되지만, OEM 리브랜드 제품처럼
/// 내부 인터페이스가 다른 경우 <see cref="Overrides"/>에 모델 규칙을 추가하면 된다.
///
/// 예) 신도 D430은 Ricoh WIM 펌웨어 → Sindoh 브랜드 + Ricoh 드라이버
/// </summary>
public static class DriverFactory
{
    private static readonly CanonDriver Canon = new();
    private static readonly RicohDriver Ricoh = new();
    private static readonly SindohDriver Sindoh = new();

    /// <summary>
    /// Brand 기본 매핑에서 벗어나는 예외 케이스.
    /// (Brand, Model 부분 일치 패턴) → 실제로 써야 하는 DriverKind.
    /// 새 모델 추가 시 여기에 한 줄만 더하면 됨.
    /// </summary>
    private static readonly (MfpBrand Brand, string ModelPattern, DriverKind Driver)[] Overrides =
    {
        (MfpBrand.Sindoh, "D430", DriverKind.Ricoh),
    };

    public static IMfpDriver? GetDriver(MfpDevice device)
    {
        var kind = ResolveDriverKind(device);
        return kind switch
        {
            DriverKind.Canon => Canon,
            DriverKind.Ricoh => Ricoh,
            DriverKind.Sindoh => Sindoh,
            _ => null,
        };
    }

    /// <summary>기기의 실제 드라이버 종류를 결정. 오버라이드 우선, 없으면 브랜드 기본.</summary>
    public static DriverKind ResolveDriverKind(MfpDevice device)
    {
        foreach (var (brand, pattern, driver) in Overrides)
        {
            if (device.Brand == brand
                && !string.IsNullOrEmpty(device.Model)
                && device.Model.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return driver;
            }
        }
        return DefaultKindForBrand(device.Brand);
    }

    private static DriverKind DefaultKindForBrand(MfpBrand brand) => brand switch
    {
        MfpBrand.Canon => DriverKind.Canon,
        MfpBrand.Ricoh => DriverKind.Ricoh,
        MfpBrand.Sindoh => DriverKind.Sindoh,
        _ => DriverKind.Unknown,
    };
}

public enum DriverKind
{
    Unknown,
    Canon,
    Ricoh,
    Sindoh,
}
