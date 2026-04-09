using System.Windows.Input;
using Scanlink.Helpers;

namespace Scanlink.ViewModels;

public class AuthViewModel : ViewModelBase
{
    private string _licenseKey = "";
    private string _errorMessage = "";

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

    public ICommand VerifyCommand { get; }

    public event Action? AuthSuccess;

    public AuthViewModel()
    {
        VerifyCommand = new RelayCommand(Verify);
    }

    private void Verify()
    {
        if (string.IsNullOrWhiteSpace(LicenseKey))
        {
            ErrorMessage = "인증 키를 입력해주세요.";
            return;
        }

        // TODO: 실제 서버 인증 구현
        // 개발 단계: 아무 값이나 입력하면 통과
        AuthSuccess?.Invoke();
    }
}
