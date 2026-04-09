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
    private string _statusMessage = "";

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string LocalFolder { get => _localFolder; set => SetProperty(ref _localFolder, value); }
    public string DeleteCycle { get => _deleteCycle; set => SetProperty(ref _deleteCycle, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

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
        StatusMessage = "저장 중...";

        var oldName = ScanBox.Name;

        // 모델 업데이트
        ScanBox.Name = Name;
        ScanBox.Password = Password;
        ScanBox.LocalFolder = LocalFolder;
        ScanBox.DeleteCycle = DeleteCycle;

        // 드라이버로 복합기 주소록 수정 (이름/비밀번호 변경)
        var driver = DriverFactory.GetDriver(Device.Brand);
        if (driver != null && ScanBox.SlotIndex >= 0)
        {
            var result = await driver.UpdateScanBoxAsync(Device, ScanBox);
            foreach (var log in result.Logs) AppLogger.Log(log);

            if (!result.Success)
            {
                // 실패 시 원래 이름 복원
                ScanBox.Name = oldName;
                Name = oldName;
                StatusMessage = $"수정 실패: {result.Message}";
                return;
            }
        }

        _scanBoxService.UpdateScanBox(ScanBox);
        StatusMessage = "저장 완료";
    }

    private void OpenFolder()
    {
        if (!string.IsNullOrEmpty(LocalFolder) && System.IO.Directory.Exists(LocalFolder))
            Process.Start("explorer.exe", LocalFolder);
    }
}
