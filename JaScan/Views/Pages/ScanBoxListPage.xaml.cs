using System.Windows.Controls;
using JaScan.ViewModels;
using JaScan.Views.Dialogs;

namespace JaScan.Views.Pages;

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
            Owner = System.Windows.Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && dialog.CreatedScanBox != null)
        {
            vm.AddScanBox(dialog.CreatedScanBox);

            // 완료 팝업
            var completeDialog = new ScanBoxCompleteDialog
            {
                Owner = System.Windows.Window.GetWindow(this)
            };
            completeDialog.ShowDialog();
        }
    }
}
