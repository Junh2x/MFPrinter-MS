using System.Diagnostics;
using System.Windows.Input;
using Scanlink.Core;
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
    private string _localFolder;
    private string _deleteCycle;
    private bool _isSaving;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string LocalFolder { get => _localFolder; set => SetProperty(ref _localFolder, value); }
    public string DeleteCycle { get => _deleteCycle; set => SetProperty(ref _deleteCycle, value); }
    public bool IsSaving { get => _isSaving; set => SetProperty(ref _isSaving, value); }

    public List<string> DeleteCycles { get; } = ["사용안함", "1일", "3일", "1주", "2주", "1개월"];

    public ICommand SaveCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand BrowseFolderCommand { get; }
    public ICommand GoBackCommand { get; }

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
        _localFolder = scanBox.LocalFolder;
        _deleteCycle = scanBox.DeleteCycle;

        SaveCommand = new RelayCommand(async () => await SaveAsync());
        OpenFolderCommand = new RelayCommand(OpenFolder);
        BrowseFolderCommand = new RelayCommand(() => RequestBrowseFolder?.Invoke());
        GoBackCommand = new RelayCommand(() => _navigation.NavigateTo(_parentPage));
    }

    private async Task SaveAsync()
    {
        IsSaving = true;

        // 원본 백업
        var oldName = ScanBox.Name;
        var oldPassword = ScanBox.Password;
        var oldLocalFolder = ScanBox.LocalFolder;
        var oldDeleteCycle = ScanBox.DeleteCycle;

        var driver = DriverFactory.GetDriver(Device);
        if (driver != null)
        {
            // 드라이버에 새 값 전달
            ScanBox.Name = Name;
            ScanBox.Password = Password;

            var result = await driver.UpdateScanBoxAsync(Device, ScanBox, oldName, oldPassword);
            foreach (var log in result.Logs) AppLogger.Log(log);

            if (!result.Success)
            {
                // 실패 → 전부 원복
                ScanBox.Name = oldName;
                ScanBox.Password = oldPassword;
                Name = oldName;
                Password = oldPassword;
                IsSaving = false;
                SaveFailed?.Invoke(result.Message);
                return;
            }
        }

        // 성공 → 로컬 데이터 반영
        ScanBox.Name = Name;
        ScanBox.Password = Password;
        ScanBox.LocalFolder = LocalFolder;
        ScanBox.DeleteCycle = DeleteCycle;
        _scanBoxService.UpdateScanBox(ScanBox);
        IsSaving = false;
        _navigation.NavigateTo(_parentPage);
    }

    public event Action<string>? SaveFailed;

    private void OpenFolder()
    {
        if (!string.IsNullOrEmpty(LocalFolder) && System.IO.Directory.Exists(LocalFolder))
            Process.Start("explorer.exe", LocalFolder);
    }
}
