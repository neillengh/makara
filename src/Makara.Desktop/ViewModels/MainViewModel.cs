using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Desktop.Models;
using Makara.Desktop.Services;
using Makara.Desktop.Views;

namespace Makara.Desktop.ViewModels;

/// <summary>
/// 主窗口 ViewModel：导航路由、服务端切换、主题切换
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly SseClient _sse;
    private readonly ServerConfigService _serverConfig;
    private readonly SettingsService _settings;
    private readonly ThemeService _theme;
    private readonly RunHistoryService _runHistory;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentPage = "dashboard";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _serverOnline;

    [ObservableProperty]
    private bool _isDarkTheme = true;

    public string PageTitle => CurrentPage switch
    {
        "dashboard" => "仪表盘",
        "workflows" => "工作流",
        "datasources" => "数据源",
        "datasets" => "数据集",
        "runs" => "执行记录",
        "servers" => "服务端",
        "settings" => "设置",
        "canvas" => "工作流画布",
        _ => "Makara"
    };

    public string CurrentServerName => CurrentServer?.Name ?? "未配置";

    public ObservableCollection<ServerProfile> Servers => _serverConfig.Servers;

    [ObservableProperty]
    private ServerProfile _currentServer;

    public MainViewModel(
        ApiClient api,
        SseClient sse,
        ServerConfigService serverConfig,
        SettingsService settings,
        ThemeService theme,
        RunHistoryService runHistory)
    {
        _api = api;
        _sse = sse;
        _serverConfig = serverConfig;
        _settings = settings;
        _theme = theme;
        _runHistory = runHistory;

        // 应用持久化的主题
        IsDarkTheme = _theme.Current != ThemeService.Light;
        if (_settings.Settings.Theme != _theme.Current)
            _theme.Apply(_settings.Settings.Theme);
        IsDarkTheme = _theme.Current == ThemeService.Dark;
        _theme.ThemeChanged += t => IsDarkTheme = t == ThemeService.Dark;

        CurrentServer = _serverConfig.Current;
        _api.SetBaseUrl(CurrentServer.Address);
        _serverConfig.CurrentServerChanged += s =>
        {
            _api.SetBaseUrl(s.Address);
            _ = RefreshServerStatusAsync();
        };

        Navigate("dashboard");
        _ = RefreshServerStatusAsync();
    }

    public async Task RefreshServerStatusAsync()
    {
        ServerOnline = await _api.CheckHealthAsync();
    }

    partial void OnCurrentServerChanged(ServerProfile value)
    {
        if (value is null) return;
        if (!ReferenceEquals(value, _serverConfig.Current))
            _serverConfig.SetCurrent(value);
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        CurrentPage = page;
        OnPropertyChanged(nameof(PageTitle));
        CurrentView = page switch
        {
            "dashboard" => new DashboardView
            {
                DataContext = new DashboardViewModel(_api, _runHistory)
            },
            "workflows" => new WorkflowsView
            {
                DataContext = new WorkflowsViewModel(_api, _runHistory) { OnEditWorkflow = OpenCanvas }
            },
            "datasources" => new DataSourcesView { DataContext = new DataSourcesViewModel(_api) },
            "datasets" => new DatasetsView { DataContext = new DatasetsViewModel(_api) },
            "runs" => new RunsView { DataContext = new RunsViewModel(_sse) },
            "servers" => new ServersView
            {
                DataContext = new ServersViewModel(_serverConfig, _api) { OnServersChanged = RefreshServerStatus }
            },
            "settings" => new SettingsView { DataContext = new SettingsViewModel(_settings, _theme) },
            _ => CurrentView
        };
    }

    private void RefreshServerStatus()
    {
        OnPropertyChanged(nameof(CurrentServerName));
        _ = RefreshServerStatusAsync();
    }

    private void OpenCanvas(string workflowId)
    {
        CurrentPage = "canvas";
        OnPropertyChanged(nameof(PageTitle));
        CurrentView = new WorkflowCanvasView
        {
            DataContext = new WorkflowCanvasViewModel(_api, workflowId)
        };
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _theme.Toggle();
        _settings.Settings.Theme = _theme.Current;
        _settings.Save();
    }
}
