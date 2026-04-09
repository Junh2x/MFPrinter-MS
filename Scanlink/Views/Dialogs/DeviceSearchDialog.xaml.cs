using System.Windows;
using System.Windows.Input;
using Scanlink.Models;

namespace Scanlink.Views.Dialogs;

public partial class DeviceSearchDialog : Window
{
    public MfpDevice? SelectedDevice { get; private set; }

    public DeviceSearchDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DeviceList.SelectionChanged += (_, _) =>
        {
            NextButton.IsEnabled = DeviceList.SelectedItem != null;
        };
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // TODO: 실제 검색 구현. 지금은 시뮬레이션.
        SearchingPanel.Visibility = Visibility.Visible;
        DeviceList.Visibility = Visibility.Collapsed;

        await Task.Delay(2000);

        // 시뮬: 검색 완료 → 빈 결과
        SearchStatusText.Text = "검색 완료. 발견된 복합기가 없습니다.";
        SearchingPanel.Visibility = Visibility.Collapsed;
        DeviceList.Visibility = Visibility.Visible;
    }

    private void ManualLink_Click(object sender, MouseButtonEventArgs e)
    {
        var dialog = new ManualConnectDialog { Owner = this };
        if (dialog.ShowDialog() == true && dialog.Device != null)
        {
            SelectedDevice = dialog.Device;
            DialogResult = true;
            Close();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (DeviceList.SelectedItem is MfpDevice device)
        {
            SelectedDevice = device;
            DialogResult = true;
            Close();
        }
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
