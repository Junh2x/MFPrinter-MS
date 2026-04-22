using Scanlink.Drivers.Canon;
using Scanlink.Drivers.Ricoh;
using Scanlink.Drivers.Sindoh;
using Scanlink.Models;

namespace Scanlink.Core;

/// <summary>
/// 드라이버 해석 진입점. 모든 Brand/Model 매핑은 <see cref="DriverRegistry"/> 에 등록되어 있고,
/// 이 Factory는 외부 호출부를 위한 얇은 래퍼.
///
/// 정적 생성자에서 기본 매핑을 등록:
///   - 각 브랜드의 기본 드라이버 (매핑되지 않은 모델에 적용)
///   - 특정 모델별 오버라이드 (예: 신도 D430은 Ricoh 드라이버 사용 — 같은 인스턴스 공유)
///
/// 새로운 모델 전용 드라이버 추가:
///   1) Drivers/{Brand}/{Brand}{Model}Driver.cs 구현
///   2) 아래 정적 생성자에 DriverRegistry.RegisterModel 한 줄 추가
/// </summary>
public static class DriverFactory
{
    static DriverFactory() => RegisterDefaults();

    /// <summary>기본 드라이버 및 모델 오버라이드를 레지스트리에 등록.
    /// 앱 시작 시 static ctor에서 1회 호출됨. 테스트에서는 상태 초기화용으로 재호출 가능.</summary>
    internal static void RegisterDefaults()
    {
        DriverRegistry.Clear();

        // 브랜드 기본 드라이버 (싱글톤) — 인스턴스를 만들어 레지스트리와 공유
        var canon = new CanonDefaultDriver();
        var ricoh = new RicohDefaultDriver();
        var sindoh = new SindohDefaultDriver();

        DriverRegistry.RegisterDefault(MfpBrand.Canon, canon);
        DriverRegistry.RegisterDefault(MfpBrand.Ricoh, ricoh);
        DriverRegistry.RegisterDefault(MfpBrand.Sindoh, sindoh);

        // 모델별 오버라이드
        // 신도 D430은 Ricoh WIM 펌웨어 기반 — 동일 Ricoh 인스턴스 재사용 (세션/캐시 공유)
        DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D430", ricoh);

        // 향후 예시:
        // DriverRegistry.RegisterModel(MfpBrand.Sindoh, @"D420", new SindohD420Driver());
        // DriverRegistry.RegisterModel(MfpBrand.Canon,  @"iR-ADV\sC3[0-9]+", new CanonGen3Driver());
    }

    /// <summary>기기에 맞는 드라이버 반환. 매핑이 없으면 null.</summary>
    public static IMfpDriver? GetDriver(MfpDevice device) => DriverRegistry.Resolve(device);
}
