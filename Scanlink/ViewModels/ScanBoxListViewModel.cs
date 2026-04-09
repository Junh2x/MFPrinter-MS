using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.Models;
using Scanlink.Services;

namespace Scanlink.ViewModels;

public class ScanBoxListViewModel : ViewModelBase
{
    private readonly ScanBoxService _scanBoxService;
    private readonly NavigationService _navigation;
    private readonly ViewModelBase _parentPage;

    public MfpDevice Device { get; }
    public ObservableCollection<ScanBox> ScanBoxes { get; }

    public bool HasScanBoxes => ScanBoxes.Count > 0;

    public ICommand AddScanBoxCommand { get; }
    public ICommand SelectScanBoxCommand { get; }
    public ICommand DeleteScanBoxCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand GoBackCommand { get; }

    public event Action? RequestAddScanBoxDialog;

    /// <summary>드라이버 작업 결과 이벤트 (로그 확인용)</summary>
    public event Action<DriverResult>? DriverResultReceived;

    public ScanBoxListViewModel(MfpDevice device, ScanBoxService scanBoxService, NavigationService navigation, ViewModelBase parentPage)
    {
        Device = device;
        _scanBoxService = scanBoxService;
        _navigation = navigation;
        _parentPage = parentPage;

        ScanBoxes = _scanBoxService.GetScanBoxesForDevice(device.Id);

        AddScanBoxCommand = new RelayCommand(() => RequestAddScanBoxDialog?.Invoke());

        SelectScanBoxCommand = new RelayCommand(param =>
        {
            if (param is ScanBox box)
            {
                var manageVm = new ScanBoxManageViewModel(box, Device, _scanBoxService, _navigation, this);
                _navigation.NavigateTo(manageVm);
            }
        });

        DeleteScanBoxCommand = new RelayCommand(param =>
        {
            if (param is ScanBox box)
                _ = DeleteScanBoxWithDriverAsync(box);
        });

        OpenFolderCommand = new RelayCommand(param =>
        {
            if (param is ScanBox box && !string.IsNullOrEmpty(box.LocalFolder) && System.IO.Directory.Exists(box.LocalFolder))
                Process.Start("explorer.exe", box.LocalFolder);
        });

        GoBackCommand = new RelayCommand(() => _navigation.NavigateTo(_parentPage));

        ScanBoxes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasScanBoxes));
    }

    /// <summary>스캔함 추가 (드라이버 호출 포함)</summary>
    public async Task<bool> AddScanBoxWithDriverAsync(ScanBox box)
    {
        box.MfpDeviceId = Device.Id;

        var driver = DriverFactory.GetDriver(Device.Brand);
        if (driver != null)
        {
            // 최초 연결 시 Setup 실행
            if (!Device.IsConfigured)
            {
                var setupResult = await driver.SetupAsync(Device);
                foreach (var log in setupResult.Logs) Debug.WriteLine(log);
                DriverResultReceived?.Invoke(setupResult);

                if (!setupResult.Success)
                    return false;
            }

            var result = await driver.AddScanBoxAsync(Device, box);
            foreach (var log in result.Logs) Debug.WriteLine(log);
            DriverResultReceived?.Invoke(result);

            if (!result.Success)
                return false;
        }

        _scanBoxService.AddScanBox(box);
        ScanBoxes.Add(box);
        OnPropertyChanged(nameof(HasScanBoxes));
        return true;
    }

    /// <summary>스캔함 삭제 (드라이버 호출 포함)</summary>
    private async Task DeleteScanBoxWithDriverAsync(ScanBox box)
    {
        var driver = DriverFactory.GetDriver(Device.Brand);
        if (driver != null && box.SlotIndex >= 0)
        {
            var result = await driver.DeleteScanBoxAsync(Device, box);
            foreach (var log in result.Logs) Debug.WriteLine(log);
            DriverResultReceived?.Invoke(result);
        }

        _scanBoxService.RemoveScanBox(box);
        ScanBoxes.Remove(box);
        OnPropertyChanged(nameof(HasScanBoxes));
    }
}
