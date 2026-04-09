using System.Windows;
using Scanlink.Helpers;
using Scanlink.Services;
using Scanlink.Views;

namespace Scanlink;

public partial class App : Application
{
    private System.Windows.Forms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 테스트 모드: dotnet run -- --test-complete
        if (e.Args.Length > 0 && e.Args[0] == "--test-complete")
        {
            var dialog = new Views.Dialogs.ScanBoxCompleteDialog(Models.MfpBrand.Ricoh);
            dialog.ShowDialog();
            Shutdown();
            return;
        }

        AppLogger.Log("APP", "Scanlink 시작");

        var authService = new AuthService();
        var authenticated = false;

        // 1. 저장된 토큰으로 자동 인증 시도
        var savedCode = authService.LoadToken();
        if (!string.IsNullOrEmpty(savedCode))
        {
            var (valid, _) = await authService.VerifyAsync(savedCode);
            if (valid)
                authenticated = true;
            else
                authService.ClearToken();
        }

        // 2. 자동 인증 실패 시 인증 창
        if (!authenticated)
        {
            var authWindow = new AuthWindow();
            authWindow.ShowDialog();
            if (!authWindow.Authenticated)
            {
                Shutdown();
                return;
            }
        }

        // 3. 메인 창
        _mainWindow = new MainWindow();
        _mainWindow.Show();

        // 4. 트레이 아이콘
        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        var iconUri = new Uri("pack://application:,,,/Assets/app.ico");
        var iconStream = Application.GetResourceStream(iconUri)?.Stream;

        _trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = "Scanlink",
            Icon = iconStream != null ? new System.Drawing.Icon(iconStream) : System.Drawing.SystemIcons.Application,
            Visible = true,
        };

        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.ShowImageMargin = false;
        menu.Renderer = new TrayMenuRenderer();

        var openItem = new System.Windows.Forms.ToolStripMenuItem("       열기       ");
        openItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
        openItem.Click += (_, _) => Dispatcher.Invoke(ShowMainWindow);
        menu.Items.Add(openItem);

        var exitItem = new System.Windows.Forms.ToolStripMenuItem("       종료       ");
        exitItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
        exitItem.Click += (_, _) => Dispatcher.Invoke(ExitApp);
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => Dispatcher.Invoke(ShowMainWindow);
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApp()
    {
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        _mainWindow?.Close();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        base.OnExit(e);
    }
}
