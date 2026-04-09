using System.Windows;
using System.Windows.Input;
using Scanlink.Models;

namespace Scanlink.Views.Dialogs;

public partial class ScanBoxAddDialog : Window
{
    private readonly MfpDevice _device;
    public ScanBox? CreatedScanBox { get; private set; }

    /// <summary>스캔함 등록 콜백. ScanBox를 받아 등록 후 에러 메시지 반환 (null=성공)</summary>
    public Func<ScanBox, Task<string?>>? RegisterCallback { get; set; }

    public ScanBoxAddDialog(MfpDevice device)
    {
        InitializeComponent();
        _device = device;

        FolderBox.Text = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Scanlink");
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("스캔함 이름을 입력해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
            NameBox.Focus();
            return;
        }

        var box = new ScanBox
        {
            Name = name,
            Password = UsePasswordCheck.IsChecked == true ? PasswordBox.Text : "",
            LocalFolder = FolderBox.Text,
            DeleteCycle = (DeleteCycleCombo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "1주",
            // NotifyOnSave 제거됨
            MfpDeviceId = _device.Id,
        };

        if (RegisterCallback != null)
        {
            // 로딩 표시
            AddButton.Visibility = Visibility.Collapsed;
            LoadingPanel.Visibility = Visibility.Visible;
            IsCloseButtonEnabled(false);

            var error = await RegisterCallback(box);

            if (error == null)
            {
                CreatedScanBox = box;
                DialogResult = true;
                Close();
            }
            else
            {
                // 에러 → 폼으로 복귀
                AddButton.Visibility = Visibility.Visible;
                LoadingPanel.Visibility = Visibility.Collapsed;
                IsCloseButtonEnabled(true);
                MessageBox.Show(error, "등록 실패", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            CreatedScanBox = box;
            DialogResult = true;
            Close();
        }
    }

    private void IsCloseButtonEnabled(bool enabled)
    {
        // 등록 중 닫기 방지
        IsEnabled = enabled || !enabled; // 항상 활성 유지 (에러 시 닫기 가능)
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
