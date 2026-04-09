using System.Windows;
using Scanlink.Views;

namespace Scanlink;

public partial class App : Application
{
    private System.Windows.Forms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. 인증 창
        var authWindow = new AuthWindow();
        authWindow.ShowDialog();
        if (!authWindow.Authenticated)
        {
            Shutdown();
            return;
        }

        // 2. 메인 창
        _mainWindow = new MainWindow();
        _mainWindow.Show();

        // 3. 트레이 아이콘
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
