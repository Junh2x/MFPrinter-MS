using System.Windows;
using System.Windows.Input;
using JaScan.ViewModels;

namespace JaScan.Views;

public partial class AuthWindow : Window
{
    public bool Authenticated { get; private set; }

    public AuthWindow()
    {
        InitializeComponent();
        var vm = new AuthViewModel();
        vm.AuthSuccess += OnAuthSuccess;
        DataContext = vm;
    }

    private void OnAuthSuccess()
    {
        Authenticated = true;
        Close();
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Authenticated = false;
        Close();
    }
}
