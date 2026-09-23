using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Desktop.Models;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

/// <summary>
/// 登录页 ViewModel：服务端地址 + 用户名 + 密码，调用 api/auth/login。
/// 登录成功后抛出 LoggedIn 事件，由 LoginWindow 关闭并打开主窗口。
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly SettingsService _settings;

    [ObservableProperty] private string _serverAddress = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private bool _rememberMe = true;
    [ObservableProperty] private string _error = string.Empty;
    [ObservableProperty] private bool _isBusy;

    /// <summary>登录成功时触发（由窗口订阅以关闭登录窗并打开 MainWindow）</summary>
    public event Action? LoggedIn;

    public LoginViewModel(ApiClient api, SettingsService settings)
    {
        _api = api;
        _settings = settings;
        ServerAddress = _settings.Settings.LastLoginServer;
        Username = _settings.Settings.LastLoginUser;
    }

    [RelayCommand]
    private async Task Login(string? password)
    {
        Error = string.Empty;
        if (string.IsNullOrWhiteSpace(ServerAddress) ||
            string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(password))
        {
            Error = "请填写服务端地址、用户名和密码";
            return;
        }

        IsBusy = true;
        try
        {
            _api.SetBaseUrl(ServerAddress.Trim());
            var result = await _api.LoginAsync(Username.Trim(), password!);
            if (result is null || !result.Success || result.User is null)
            {
                Error = result?.Message ?? "登录失败，无法连接服务端";
                return;
            }

            Session.Token = result.Token;
            Session.CurrentUser = result.User;

            if (RememberMe)
            {
                _settings.Settings.LastLoginServer = ServerAddress.Trim();
                _settings.Settings.LastLoginUser = Username.Trim();
                _settings.Save();
            }

            LoggedIn?.Invoke();
        }
        catch (Exception ex)
        {
            Error = "登录异常：" + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ForgotPassword()
    {
        Error = "请联系系统管理员重置密码";
    }
}
