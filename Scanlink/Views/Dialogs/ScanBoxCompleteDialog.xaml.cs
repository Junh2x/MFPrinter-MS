using System.Windows;

namespace Scanlink.Views.Dialogs;

public partial class ScanBoxCompleteDialog : Window
{
    public ScanBoxCompleteDialog()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
