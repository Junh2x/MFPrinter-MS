using System.Collections.ObjectModel;
using System.Windows.Input;
using JaScan.Helpers;
using JaScan.Models;
using JaScan.Services;

namespace JaScan.ViewModels;

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
            {
                _scanBoxService.RemoveScanBox(box);
                ScanBoxes.Remove(box);
                OnPropertyChanged(nameof(HasScanBoxes));
            }
        });

        OpenFolderCommand = new RelayCommand(param =>
        {
            if (param is ScanBox box && !string.IsNullOrEmpty(box.LocalFolder) && System.IO.Directory.Exists(box.LocalFolder))
                System.Diagnostics.Process.Start("explorer.exe", box.LocalFolder);
        });

        GoBackCommand = new RelayCommand(() => _navigation.NavigateTo(_parentPage));

        ScanBoxes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasScanBoxes));
    }

    public void AddScanBox(ScanBox box)
    {
        box.MfpDeviceId = Device.Id;
        _scanBoxService.AddScanBox(box);
        ScanBoxes.Add(box);
        OnPropertyChanged(nameof(HasScanBoxes));
    }
}
