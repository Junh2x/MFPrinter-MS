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
            oldVm.RequestAddScanBoxDialog -= ShowAddDialog;
        if (e.NewValue is ScanBoxListViewModel newVm)
            newVm.RequestAddScanBoxDialog += ShowAddDialog;
    }

    private async void ShowAddDialog()
    {
        if (DataContext is not ScanBoxListViewModel vm) return;

        var dialog = new ScanBoxAddDialog(vm.Device)
        {
            Owner = System.Windows.Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && dialog.CreatedScanBox != null)
        {
            var error = await vm.AddScanBoxWithDriverAsync(dialog.CreatedScanBox);

            if (error == null)
            {
                var completeDialog = new ScanBoxCompleteDialog
                {
                    Owner = System.Windows.Window.GetWindow(this)
                };
                completeDialog.ShowDialog();
            }
            else
            {
                MessageBox.Show(
                    error,
                    "스캔함 추가 실패",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
