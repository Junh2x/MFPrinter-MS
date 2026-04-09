using System.Windows;
using System.Windows.Input;
using Scanlink.Models;
using Scanlink.Services;

namespace Scanlink.Views.Dialogs;

public partial class DeviceSearchDialog : Window
{
    private readonly DeviceDiscoveryService _discovery = new();
    private CancellationTokenSource? _cts;

    /// <summary>이미 추가된 기기 IP 목록 (검색 결과에서 제외)</summary>
    public List<string> ExistingIps { get; set; } = [];

    public MfpDevice? SelectedDevice { get; private set; }

    public DeviceSearchDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += (_, _) => _cts?.Cancel();
        DeviceList.SelectionChanged += (_, _) =>
        {
            NextButton.IsEnabled = DeviceList.SelectedItem != null;
        };
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        SearchingPanel.Visibility = Visibility.Visible;
        DeviceList.Visibility = Visibility.Collapsed;
        SearchStatusText.Text = "네트워크 스캔 중...";
        _cts = new CancellationTokenSource();

        try
        {
            var devices = await _discovery.ScanSubnetAsync(
                progressCallback: (scanned, total) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        SearchStatusText.Text = $"네트워크 스캔 중... ({scanned}/{total})";
                    });
                },
                ct: _cts.Token);

            // 이미 추가된 기기 제외
            var filtered = devices.Where(d => !ExistingIps.Contains(d.Ip)).ToList();

            if (filtered.Count > 0)
            {
                SearchingPanel.Visibility = Visibility.Collapsed;
                DeviceList.ItemsSource = filtered;
                DeviceList.Visibility = Visibility.Visible;
            }
            else
            {
                SearchStatusText.Text = devices.Count > 0
                    ? "모든 복합기가 이미 등록되어 있습니다."
                    : "발견된 복합기가 없습니다.";
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            SearchStatusText.Text = $"검색 오류: {ex.Message}";
        }
    }

    private void ManualLink_Click(object sender, MouseButtonEventArgs e)
    {
        _cts?.Cancel();
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
        _cts?.Cancel();
        DialogResult = false;
        Close();
    }
}
