using System.Windows.Controls;
using Scanlink.ViewModels;
using Scanlink.Views.Dialogs;

namespace Scanlink.Views.Pages;

public partial class ScanBoxListPage : UserControl
{
    public ScanBoxListPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ScanBoxListViewModel oldVm)
        {
            oldVm.RequestAddScanBoxDialog -= ShowAddDialog;
            oldVm.DeleteFailed -= ShowDeleteError;
        }
        if (e.NewValue is ScanBoxListViewModel newVm)
        {
            newVm.RequestAddScanBoxDialog += ShowAddDialog;
            newVm.DeleteFailed += ShowDeleteError;
        }
    }

    private void ShowDeleteError(string message)
    {
        MessageBox.Show(message, "삭제 실패",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }

    private void ShowAddDialog()
    {
        if (DataContext is not ScanBoxListViewModel vm) return;

        var dialog = new ScanBoxAddDialog(vm.Device)
        {
            Owner = System.Windows.Window.GetWindow(this),
            RegisterCallback = vm.AddScanBoxWithDriverAsync,
        };

        if (dialog.ShowDialog() == true && dialog.CreatedScanBox != null)
        {
            var completeDialog = new ScanBoxCompleteDialog(vm.Device.Brand)
            {
                Owner = System.Windows.Window.GetWindow(this)
            };
            completeDialog.ShowDialog();
        }
    }
}
