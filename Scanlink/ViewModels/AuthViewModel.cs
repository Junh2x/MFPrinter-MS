using System.Windows.Input;
using Scanlink.Helpers;
using Scanlink.Services;

namespace Scanlink.ViewModels;

public class AuthViewModel : ViewModelBase
{
    private readonly AuthService _authService = new();

    private string _licenseKey = "";
    private string _errorMessage = "";
    private bool _isVerifying;

    public string LicenseKey
    {
        get => _licenseKey;
        set
        {
            SetProperty(ref _licenseKey, value);
            ErrorMessage = "";
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsVerifying
    {
        get => _isVerifying;
        set => SetProperty(ref _isVerifying, value);
    }

    public ICommand VerifyCommand { get; }

    public event Action? AuthSuccess;

    public AuthViewModel()
    {
        VerifyCommand = new RelayCommand(async () => await VerifyAsync(), () => !IsVerifying);
    }

    private async Task VerifyAsync()
    {
        if (string.IsNullOrWhiteSpace(LicenseKey))
        {
            ErrorMessage = "인증 코드를 입력해주세요.";
            return;
        }

        IsVerifying = true;
        ErrorMessage = "";

        var (valid, message) = await _authService.VerifyAsync(LicenseKey.Trim());

        if (valid)
        {
            _authService.SaveToken(LicenseKey.Trim());
            AuthSuccess?.Invoke();
        }
        else
        {
            ErrorMessage = message;
        }

        IsVerifying = false;
    }
}
