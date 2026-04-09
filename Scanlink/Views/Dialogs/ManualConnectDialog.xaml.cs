using System.Net;
using System.Windows;
using System.Windows.Input;
using Scanlink.Models;
using Scanlink.Services;

namespace Scanlink.Views.Dialogs;

public partial class ManualConnectDialog : Window
{
    private readonly DeviceDiscoveryService _discovery = new();
    public MfpDevice? Device { get; private set; }

    public ManualConnectDialog()
    {
        InitializeComponent();
        IpTextBox.Focus();
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        var ip = IpTextBox.Text.Trim();

        if (!IPAddress.TryParse(ip, out _))
        {
            ErrorText.Text = "올바른 IP 주소를 입력해주세요.";
            return;
        }

        ErrorText.Text = "기기 식별 중...";
        IpTextBox.IsEnabled = false;

        try
        {
            var device = await _discovery.IdentifyDeviceAsync(ip);

            if (device != null)
            {
                Device = device;
                DialogResult = true;
                Close();
            }
            else
            {
                // SNMP 응답 직접 확인하여 상세 에러
                var sysDescr = await DeviceDiscoveryService.SnmpGetSysDescrAsync(ip);
                if (sysDescr != null)
                    ErrorText.Text = $"지원하지 않는 기기입니다.\nSNMP: {sysDescr}";
                else
                    ErrorText.Text = "기기가 응답하지 않습니다. IP를 확인하세요.";
            }
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"연결 실패: {ex.Message}";
        }
        finally
        {
            IpTextBox.IsEnabled = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
