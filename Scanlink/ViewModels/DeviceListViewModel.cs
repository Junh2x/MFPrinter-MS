using System.Collections.ObjectModel;
using System.Windows.Input;
using Scanlink.Helpers;
using Scanlink.Models;
using Scanlink.Services;

namespace Scanlink.ViewModels;

public class DeviceListViewModel : ViewModelBase
{
    private readonly DeviceService _deviceService;
    private readonly ScanBoxService _scanBoxService;
    private readonly NavigationService _navigation;

    public ObservableCollection<MfpDevice> Devices => _deviceService.Devices;

    public bool HasDevices => Devices.Count > 0;

    public ICommand SelectDeviceCommand { get; }
    public ICommand RemoveDeviceCommand { get; }

    public event Action? RequestAutoSearchDialog;
    public event Action? RequestManualConnectDialog;

    public ICommand AutoSearchCommand { get; }
    public ICommand ManualConnectCommand { get; }

    public DeviceListViewModel(DeviceService deviceService, ScanBoxService scanBoxService, NavigationService navigation)
    {
        _deviceService = deviceService;
        _scanBoxService = scanBoxService;
        _navigation = navigation;

        AutoSearchCommand = new RelayCommand(() => RequestAutoSearchDialog?.Invoke());
        ManualConnectCommand = new RelayCommand(() => RequestManualConnectDialog?.Invoke());

        SelectDeviceCommand = new RelayCommand(param =>
        {
            if (param is MfpDevice device)
            {
                var scanBoxListVm = new ScanBoxListViewModel(device, _scanBoxService, _navigation, this);
                _navigation.NavigateTo(scanBoxListVm);
            }
        });

        RemoveDeviceCommand = new RelayCommand(param =>
        {
            if (param is MfpDevice device)
            {
                _deviceService.RemoveDevice(device);
                OnPropertyChanged(nameof(HasDevices));
            }
        });

        Devices.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasDevices));
    }

    public void AddDevice(MfpDevice device)
    {
        // 중복 IP 방지
        foreach (var d in Devices)
        {
            if (d.Ip == device.Ip)
            {
                d.Brand = device.Brand;
                d.Model = device.Model;
                d.Port = device.Port;
                d.BaseUrl = device.BaseUrl;
                d.Status = device.Status;
                return;
            }
        }

        _deviceService.AddDevice(device);
        OnPropertyChanged(nameof(HasDevices));
    }
}
