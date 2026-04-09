using System.Windows;
using System.Windows.Controls;
using Scanlink.Core;
using Scanlink.Helpers;
using Scanlink.ViewModels;
using Scanlink.Views.Dialogs;

namespace Scanlink.Views.Pages;

public partial class DeviceListPage : UserControl
{
    public DeviceListPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is DeviceListViewModel oldVm)
        {
            oldVm.RequestAutoSearchDialog -= ShowAutoSearchDialog;
            oldVm.RequestManualConnectDialog -= ShowManualConnectDialog;
        }
        if (e.NewValue is DeviceListViewModel newVm)
        {
            newVm.RequestAutoSearchDialog += ShowAutoSearchDialog;
            newVm.RequestManualConnectDialog += ShowManualConnectDialog;
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        AddPopup.IsOpen = !AddPopup.IsOpen;
    }

    private void AutoSearch_Click(object sender, RoutedEventArgs e)
    {
        AddPopup.IsOpen = false;
        ShowAutoSearchDialog();
    }

    private void ManualConnect_Click(object sender, RoutedEventArgs e)
    {
        AddPopup.IsOpen = false;
        ShowManualConnectDialog();
    }

    private async void ShowAutoSearchDialog()
    {
        var dialog = new DeviceSearchDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.SelectedDevice != null)
        {
            if (DataContext is DeviceListViewModel vm)
            {
                await SetupAndAddDevice(vm, dialog.SelectedDevice);
            }
        }
    }

    private async void ShowManualConnectDialog()
    {
        var dialog = new ManualConnectDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Device != null)
        {
            if (DataContext is DeviceListViewModel vm)
            {
                await SetupAndAddDevice(vm, dialog.Device);
            }
        }
    }

    /// <summary>기기 등록 시 브랜드별 초기 설정 실행 후 추가</summary>
    private async Task SetupAndAddDevice(DeviceListViewModel vm, Models.MfpDevice device)
    {
        var driver = DriverFactory.GetDriver(device.Brand);
        if (driver != null)
        {
            var result = await driver.SetupAsync(device);
            foreach (var log in result.Logs) AppLogger.Log(log);

            if (!result.Success)
            {
                MessageBox.Show(
                    $"기기 초기 설정 실패:\n{result.Message}",
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        vm.AddDevice(device);
    }
}
