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
