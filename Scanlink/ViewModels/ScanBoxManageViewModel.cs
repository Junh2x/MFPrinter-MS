using System.Diagnostics;
using System.Windows.Input;
using Scanlink.Helpers;
using Scanlink.Models;
using Scanlink.Services;

namespace Scanlink.ViewModels;

public class ScanBoxManageViewModel : ViewModelBase
{
    private readonly ScanBoxService _scanBoxService;
    private readonly NavigationService _navigation;
    private readonly ViewModelBase _parentPage;

    public ScanBox ScanBox { get; }
    public MfpDevice Device { get; }

    private string _name;
    private string _password;
    private string _fileFormat;
    private string _colorMode;
    private int _resolution;
    private string _localFolder;
    private bool _notifyOnSave;
    private string _deleteCycle;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string FileFormat { get => _fileFormat; set => SetProperty(ref _fileFormat, value); }
    public string ColorMode { get => _colorMode; set => SetProperty(ref _colorMode, value); }
    public int Resolution { get => _resolution; set => SetProperty(ref _resolution, value); }
    public string LocalFolder { get => _localFolder; set => SetProperty(ref _localFolder, value); }
    public bool NotifyOnSave { get => _notifyOnSave; set => SetProperty(ref _notifyOnSave, value); }
    public string DeleteCycle { get => _deleteCycle; set => SetProperty(ref _deleteCycle, value); }

    public List<string> FileFormats { get; } = ["PDF", "PDF 압축", "TIFF", "JPEG"];
    public List<string> ColorModes { get; } = ["자동컬러", "컬러", "흑백"];
    public List<int> Resolutions { get; } = [100, 200, 300, 600];
    public List<string> DeleteCycles { get; } = ["사용안함", "1일", "3일", "1주", "2주", "1개월"];

    public ICommand SaveCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand BrowseFolderCommand { get; }
    public ICommand GoBackCommand { get; }

    // View에서 FolderBrowserDialog 연결용
    public event Action? RequestBrowseFolder;

    public ScanBoxManageViewModel(ScanBox scanBox, MfpDevice device, ScanBoxService scanBoxService, NavigationService navigation, ViewModelBase parentPage)
    {
        ScanBox = scanBox;
        Device = device;
        _scanBoxService = scanBoxService;
        _navigation = navigation;
        _parentPage = parentPage;

        _name = scanBox.Name;
        _password = scanBox.Password;
        _fileFormat = scanBox.FileFormat;
        _colorMode = scanBox.ColorMode;
        _resolution = scanBox.Resolution;
        _localFolder = scanBox.LocalFolder;
        _notifyOnSave = scanBox.NotifyOnSave;
        _deleteCycle = scanBox.DeleteCycle;

        SaveCommand = new RelayCommand(Save);
        OpenFolderCommand = new RelayCommand(OpenFolder);
        BrowseFolderCommand = new RelayCommand(() => RequestBrowseFolder?.Invoke());
        GoBackCommand = new RelayCommand(() => _navigation.NavigateTo(_parentPage));
    }

    private void Save()
    {
        ScanBox.Name = Name;
        ScanBox.Password = Password;
        ScanBox.FileFormat = FileFormat;
        ScanBox.ColorMode = ColorMode;
        ScanBox.Resolution = Resolution;
        ScanBox.LocalFolder = LocalFolder;
        ScanBox.NotifyOnSave = NotifyOnSave;
        ScanBox.DeleteCycle = DeleteCycle;

        _scanBoxService.UpdateScanBox(ScanBox);
    }

    private void OpenFolder()
    {
        if (!string.IsNullOrEmpty(LocalFolder) && System.IO.Directory.Exists(LocalFolder))
            Process.Start("explorer.exe", LocalFolder);
    }
}
