using System.Windows;
using System.Windows.Controls;
using Scanlink.ViewModels;

namespace Scanlink.Views.Pages;

public partial class ScanBoxManagePage : UserControl
{
    public ScanBoxManagePage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ScanBoxManageViewModel oldVm)
        {
            oldVm.RequestBrowseFolder -= BrowseFolder;
            oldVm.SaveFailed -= ShowSaveError;
        }
        if (e.NewValue is ScanBoxManageViewModel newVm)
        {
            newVm.RequestBrowseFolder += BrowseFolder;
            newVm.SaveFailed += ShowSaveError;
        }
    }

    private void BrowseFolder()
    {
        if (DataContext is not ScanBoxManageViewModel vm) return;
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "스캔 파일 저장 위치를 선택하세요.",
            SelectedPath = vm.LocalFolder,
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            vm.LocalFolder = dialog.SelectedPath;
    }

    private void ShowSaveError(string message)
    {
        MessageBox.Show(message, "저장 실패", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
