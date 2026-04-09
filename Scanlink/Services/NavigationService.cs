using Scanlink.Helpers;

namespace Scanlink.Services;

/// <summary>
/// 메인 창 내 페이지 전환 서비스
/// </summary>
public class NavigationService : ViewModelBase
{
    private ViewModelBase? _currentPage;
    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public void NavigateTo(ViewModelBase page)
    {
        CurrentPage = page;
    }
}
