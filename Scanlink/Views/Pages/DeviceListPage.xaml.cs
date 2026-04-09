using System.Windows;
using System.Windows.Controls;
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

    private void ShowAutoSearchDialog()
    {
        var dialog = new DeviceSearchDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.SelectedDevice != null)
        {
            if (DataContext is DeviceListViewModel vm)
                vm.AddDevice(dialog.SelectedDevice);
        }
    }

    private void ShowManualConnectDialog()
    {
        var dialog = new ManualConnectDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Device != null)
        {
            if (DataContext is DeviceListViewModel vm)
                vm.AddDevice(dialog.Device);
        }
    }
}
