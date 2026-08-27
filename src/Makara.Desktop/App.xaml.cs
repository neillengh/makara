using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Makara.Desktop.Services;
using Makara.Desktop.ViewModels;

namespace Makara.Desktop;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ApiClient>();
        services.AddSingleton<SseClient>();
        services.AddSingleton<MainViewModel>();
        _services = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow
        {
            DataContext = _services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}
