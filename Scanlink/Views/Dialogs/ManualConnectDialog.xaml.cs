using System.Net;
using System.Windows;
using System.Windows.Input;
using Scanlink.Models;

namespace Scanlink.Views.Dialogs;

public partial class ManualConnectDialog : Window
{
    public MfpDevice? Device { get; private set; }

    public ManualConnectDialog()
    {
        InitializeComponent();
        IpTextBox.Focus();
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        var ip = IpTextBox.Text.Trim();

        if (!IPAddress.TryParse(ip, out _))
        {
            ErrorText.Text = "올바른 IP 주소를 입력해주세요.";
            return;
        }

        ErrorText.Text = "";

        // TODO: 실제 연결 시도 + 브랜드 식별
        // 지금은 수동으로 추가만 함
        Device = new MfpDevice
        {
            Ip = ip,
            Brand = MfpBrand.Unknown,
            Model = "(수동 연결)",
            Status = ConnectionStatus.Connected,
        };

        DialogResult = true;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
