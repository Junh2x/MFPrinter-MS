using System.Windows;
using System.Windows.Media.Imaging;
using Scanlink.Models;

namespace Scanlink.Views.Dialogs;

public partial class ScanBoxCompleteDialog : Window
{
    public ScanBoxCompleteDialog(MfpBrand brand = MfpBrand.Unknown)
    {
        InitializeComponent();
        SetManualImage(brand);
    }

    private void SetManualImage(MfpBrand brand)
    {
        var imagePath = brand switch
        {
            MfpBrand.Ricoh  => "pack://application:,,,/Assets/ricoh_manual.png",
            MfpBrand.Canon  => "pack://application:,,,/Assets/canon_manual.png",
            MfpBrand.Sindoh => "pack://application:,,,/Assets/sindoh_manual.png",
            _ => null,
        };

        if (imagePath != null)
        {
            ManualImage.Source = new BitmapImage(new Uri(imagePath));
            ManualImageBorder.Visibility = Visibility.Visible;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
