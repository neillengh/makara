using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Makara.Desktop.Services;
using Makara.Desktop.ViewModels;
using Makara.Desktop.Views;

namespace Makara.Desktop;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ApiClient>();
        services.AddSingleton<SseClient>();
        services.AddSingleton<ServerConfigService>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<RunHistoryService>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        _services = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 先显示登录窗，登录成功后再打开主窗口
        var loginWindow = new LoginWindow
        {
            DataContext = _services.GetRequiredService<LoginViewModel>()
        };

        if (loginWindow.ShowDialog() == true)
        {
            var mainWindow = new MainWindow
            {
                DataContext = _services.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }
        else
        {
            // 登录窗被关闭/取消 → 退出应用
            Shutdown();
        }
    }
}
