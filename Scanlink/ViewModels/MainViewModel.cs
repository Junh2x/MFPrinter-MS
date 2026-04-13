using System.Windows.Input;
using Scanlink.Helpers;
using Scanlink.Services;

namespace Scanlink.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;
    private readonly DeviceService _deviceService;
    private readonly ScanBoxService _scanBoxService;
    private readonly FileWatchService _fileWatchService;

    private string _currentPageName = "devices";

    public string CurrentPageName
    {
        get => _currentPageName;
        set => SetProperty(ref _currentPageName, value);
    }

    public ViewModelBase? CurrentPage => _navigation.CurrentPage;

    public DeviceListViewModel DeviceListVm { get; }
    public ICommand NavigateCommand { get; }

    public MainViewModel()
    {
        _navigation = new NavigationService();
        _deviceService = new DeviceService();
        _scanBoxService = new ScanBoxService();

        DeviceListVm = new DeviceListViewModel(_deviceService, _scanBoxService, _navigation);

        _navigation.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NavigationService.CurrentPage))
                OnPropertyChanged(nameof(CurrentPage));
        };

        NavigateCommand = new RelayCommand(Navigate);

        // 초기 페이지
        _navigation.NavigateTo(DeviceListVm);

        // 파일 감시 서비스 (5초 간격, 백그라운드)
        _fileWatchService = new FileWatchService(_deviceService, _scanBoxService, TimeSpan.FromSeconds(5));
        _fileWatchService.Start();
    }

    private void Navigate(object? param)
    {
        var page = param as string ?? "";
        CurrentPageName = page;

        switch (page)
        {
            case "devices":
                _navigation.NavigateTo(DeviceListVm);
                break;
        }
    }
}
