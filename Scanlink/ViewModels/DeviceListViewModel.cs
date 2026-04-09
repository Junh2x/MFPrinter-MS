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

    // View에서 처리할 이벤트
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

        // 더미 데이터 (개발용)
        LoadDummyData();
    }

    public void AddDevice(MfpDevice device)
    {
        _deviceService.AddDevice(device);
        OnPropertyChanged(nameof(HasDevices));
    }

    private void LoadDummyData()
    {
        _deviceService.AddDevice(new MfpDevice
        {
            Ip = "192.168.11.227",
            Brand = MfpBrand.Canon,
            Model = "iR-ADV C3530",
            Port = 8000,
            BaseUrl = "http://192.168.11.227:8000",
            Status = ConnectionStatus.Connected,
        });
        _deviceService.AddDevice(new MfpDevice
        {
            Ip = "192.168.11.185",
            Brand = MfpBrand.Ricoh,
            Model = "IM C2010",
            Port = 80,
            BaseUrl = "http://192.168.11.185",
            Status = ConnectionStatus.Connected,
        });
        _deviceService.AddDevice(new MfpDevice
        {
            Ip = "192.168.11.228",
            Brand = MfpBrand.Sindoh,
            Model = "D420",
            Port = 80,
            BaseUrl = "http://192.168.11.228",
            Status = ConnectionStatus.Connected,
        });
    }
}
