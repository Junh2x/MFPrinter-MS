using System.Text.RegularExpressions;
using Scanlink.Models;

namespace Scanlink.Core;

/// <summary>
/// Brand + Model 기반 드라이버 레지스트리.
///
/// 두 단계 매핑:
///   1) Model 규칙 (특정 모델 → 전용 드라이버). Brand+Model 정규식 매칭.
///   2) Brand 기본값 (매핑되지 않은 모델 → 해당 브랜드의 기본 드라이버).
///
/// 드라이버 인스턴스는 Register 시점에 생성되어 등록되며, 하나의 인스턴스를 여러 키에
/// 등록할 수 있어 예: RicohDefaultDriver 하나를 Ricoh 기본 + Sindoh D430 모두에 재사용.
///
/// 새 모델 지원 = DriverFactory 정적 생성자에 <see cref="RegisterModel"/> 한 줄 추가.
/// </summary>
public static class DriverRegistry
{
    private sealed record ModelRule(MfpBrand Brand, Regex Pattern, IMfpDriver Driver);

    private static readonly List<ModelRule> _modelRules = new();
    private static readonly Dictionary<MfpBrand, IMfpDriver> _defaults = new();
    private static readonly object _sync = new();

    /// <summary>브랜드 기본 드라이버 등록.</summary>
    public static void RegisterDefault(MfpBrand brand, IMfpDriver driver)
    {
        lock (_sync) { _defaults[brand] = driver; }
    }

    /// <summary>특정 모델 패턴에 대한 드라이버 등록. 등록 순서대로 우선순위.</summary>
    public static void RegisterModel(MfpBrand brand, string modelPattern, IMfpDriver driver)
    {
        lock (_sync)
        {
            _modelRules.Add(new ModelRule(
                brand,
                new Regex(modelPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled),
                driver));
        }
    }

    /// <summary>기기에 맞는 드라이버 해석. 모델 규칙 우선, 없으면 브랜드 기본.</summary>
    public static IMfpDriver? Resolve(MfpDevice device)
    {
        lock (_sync)
        {
            if (!string.IsNullOrEmpty(device.Model))
            {
                foreach (var rule in _modelRules)
                {
                    if (rule.Brand == device.Brand && rule.Pattern.IsMatch(device.Model))
                        return rule.Driver;
                }
            }

            return _defaults.TryGetValue(device.Brand, out var def) ? def : null;
        }
    }

    /// <summary>등록된 모든 드라이버 인스턴스(중복 제거). 세션 정리 등에 사용.</summary>
    public static IReadOnlyList<IMfpDriver> AllRegistered()
    {
        lock (_sync)
        {
            var seen = new HashSet<IMfpDriver>();
            var result = new List<IMfpDriver>();

            foreach (var d in _defaults.Values)
                if (seen.Add(d)) result.Add(d);

            foreach (var rule in _modelRules)
                if (seen.Add(rule.Driver)) result.Add(rule.Driver);

            return result;
        }
    }

    /// <summary>앱 종료 시 등록된 모든 드라이버의 세션을 정리.</summary>
    public static void DisposeAllSessions()
    {
        foreach (var driver in AllRegistered())
        {
            try { driver.DisposeSessions(); }
            catch { /* 정리 중 실패는 무시 */ }
        }
    }

    /// <summary>테스트 전용: 모든 등록 초기화. 런타임 호출 금지.</summary>
    internal static void Clear()
    {
        lock (_sync)
        {
            _modelRules.Clear();
            _defaults.Clear();
        }
    }
}
