using System.Windows;
using System.Windows.Input;
using JaScan.Models;

namespace JaScan.Views.Dialogs;

public partial class ScanBoxAddDialog : Window
{
    private readonly MfpDevice _device;
    public ScanBox? CreatedScanBox { get; private set; }

    public ScanBoxAddDialog(MfpDevice device)
    {
        InitializeComponent();
        _device = device;

        FolderBox.Text = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Scanlink");
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("스캔함 이름을 입력해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
            NameBox.Focus();
            return;
        }

        CreatedScanBox = new ScanBox
        {
            Name = name,
            Password = UsePasswordCheck.IsChecked == true ? PasswordBox.Text : "",
            LocalFolder = FolderBox.Text,
            NotifyOnSave = NotifyCheck.IsChecked == true,
            DeleteCycle = (DeleteCycleCombo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "1주",
            MfpDeviceId = _device.Id,
        };

        DialogResult = true;
        Close();
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "스캔 파일 저장 위치를 선택하세요.",
            SelectedPath = FolderBox.Text,
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            FolderBox.Text = dialog.SelectedPath;
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
