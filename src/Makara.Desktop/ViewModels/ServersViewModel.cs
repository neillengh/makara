using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Desktop.Models;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

/// <summary>
/// 服务端列表项 ViewModel
/// </summary>
public partial class ServerItemViewModel : ObservableObject
{
    public ServerProfile Model { get; }

    [ObservableProperty] private bool _isOnline;
    [ObservableProperty] private bool _isCurrent;
    [ObservableProperty] private int _latencyMs = -1;

    public ServerItemViewModel(ServerProfile model) => Model = model;

    public string Name => Model.Name;
    public string Address => Model.Address;
    public bool IsDefault => Model.IsDefault;
}

/// <summary>
/// 服务端管理页 ViewModel
/// </summary>
public partial class ServersViewModel : ObservableObject
{
    private readonly ServerConfigService _config;
    private readonly ApiClient _api;

    public ObservableCollection<ServerItemViewModel> Servers { get; } = [];

    [ObservableProperty] private ServerItemViewModel? _selectedServer;
    [ObservableProperty] private ServerItemViewModel? _editingServer;

    // 编辑表单字段
    [ObservableProperty] private string _editName = string.Empty;
    [ObservableProperty] private string _editAddress = string.Empty;
    [ObservableProperty] private string _editDescription = string.Empty;
    [ObservableProperty] private string _editToken = string.Empty;
    [ObservableProperty] private bool _editIsDefault;

    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _connectionText = "未连接";
    [ObservableProperty] private string _latencyText = string.Empty;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private bool _isTesting;

    /// <summary>通知主窗口刷新服务端状态（切换后触发）</summary>
    public Action? OnServersChanged { get; set; }

    public ServersViewModel(ServerConfigService config, ApiClient api)
    {
        _config = config;
        _api = api;

        foreach (var s in config.Servers)
        {
            Servers.Add(new ServerItemViewModel(s)
            {
                IsCurrent = ReferenceEquals(s, config.Current)
            });
        }
        SelectedServer = Servers.FirstOrDefault();
        _ = CheckAllAsync();
    }

    partial void OnSelectedServerChanged(ServerItemViewModel? value)
    {
        if (value is null) return;
        EditingServer = value;
        EditName = value.Model.Name;
        EditAddress = value.Model.Address;
        EditDescription = value.Model.Description;
        EditToken = value.Model.Token;
        EditIsDefault = value.Model.IsDefault;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task CheckAllAsync()
    {
        foreach (var server in Servers)
        {
            var (ok, _) = await PingAsync(server.Model.Address);
            server.IsOnline = ok;
        }
        RefreshConnectionText();
    }

    /// <summary>测连接（使用表单中的地址）</summary>
    [RelayCommand]
    private async Task TestAsync()
    {
        if (string.IsNullOrWhiteSpace(EditAddress))
        {
            StatusMessage = "请输入服务端地址";
            return;
        }
        IsTesting = true;
        StatusMessage = "正在测试连接...";
        try
        {
            var (ok, latency) = await PingAsync(EditAddress);
            if (ok)
            {
                StatusMessage = $"连接成功，延迟 {latency}ms";
                if (EditingServer is not null)
                {
                    EditingServer.IsOnline = true;
                    EditingServer.LatencyMs = latency;
                }
            }
            else
            {
                StatusMessage = "连接失败：服务端无响应";
                if (EditingServer is not null) EditingServer.IsOnline = false;
            }
        }
        finally
        {
            IsTesting = false;
            RefreshConnectionText();
        }
    }

    private static async Task<(bool Ok, int LatencyMs)> PingAsync(string address)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var baseUri = address.TrimEnd('/');
            var resp = await http.GetAsync($"{baseUri}/api/system/health", cts.Token);
            return (resp.IsSuccessStatusCode, (int)sw.ElapsedMilliseconds);
        }
        catch
        {
            return (false, -1);
        }
    }

    [RelayCommand]
    private void Add()
    {
        var profile = new ServerProfile
        {
            Name = "新服务端",
            Address = "http://",
            Description = string.Empty
        };
        _config.Add(profile);
        var vm = new ServerItemViewModel(profile);
        Servers.Add(vm);
        SelectedServer = vm;
        StatusMessage = "已添加服务端，请填写信息后保存";
        OnServersChanged?.Invoke();
    }

    [RelayCommand]
    private void Save()
    {
        if (EditingServer is null) return;
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditAddress))
        {
            StatusMessage = "名称和地址不能为空";
            return;
        }

        EditingServer.Model.Name = EditName.Trim();
        EditingServer.Model.Address = EditAddress.Trim().TrimEnd('/');
        EditingServer.Model.Description = EditDescription;
        EditingServer.Model.Token = EditToken;

        if (EditIsDefault)
            _config.SetDefault(EditingServer.Model);

        _config.Save();
        RebuildList();
        StatusMessage = "已保存";
        OnServersChanged?.Invoke();
    }

    [RelayCommand]
    private void SetDefault()
    {
        if (EditingServer is null) return;
        _config.SetDefault(EditingServer.Model);
        _config.SetCurrent(EditingServer.Model);
        EditIsDefault = true;
        RebuildList();
        StatusMessage = $"已将「{EditingServer.Model.Name}」设为默认服务端";
        OnServersChanged?.Invoke();
    }

    /// <summary>连接当前服务端（选中即切换）</summary>
    [RelayCommand]
    private void Connect(ServerItemViewModel server)
    {
        _config.SetCurrent(server.Model);
        _api.SetBaseUrl(server.Model.Address);
        foreach (var s in Servers) s.IsCurrent = ReferenceEquals(s, server);
        RebuildList();
        StatusMessage = $"已切换到「{server.Model.Name}」";
        OnServersChanged?.Invoke();
        _ = server.IsOnline ? Task.CompletedTask : CheckAllAsync();
    }

    [RelayCommand]
    private void Delete()
    {
        if (EditingServer is null) return;
        var toDelete = EditingServer;
        _config.Remove(toDelete.Model);
        Servers.Remove(toDelete);
        SelectedServer = Servers.FirstOrDefault();
        RebuildList();
        StatusMessage = "已删除";
        OnServersChanged?.Invoke();
    }

    private void RebuildList()
    {
        for (var i = 0; i < Servers.Count; i++)
        {
            Servers[i].IsCurrent = ReferenceEquals(Servers[i].Model, _config.Current);
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Servers)));
        }
        OnPropertyChanged(nameof(Servers));
        RefreshConnectionText();
    }

    private void RefreshConnectionText()
    {
        var current = Servers.FirstOrDefault(s => s.IsCurrent);
        IsConnected = current is { IsOnline: true };
        ConnectionText = current is null
            ? "未连接"
            : $"{(current.IsOnline ? "已连接" : "连接异常")}：{current.Name}";
        LatencyText = current is { IsOnline: true } && current.LatencyMs >= 0
            ? $"延迟 {current.LatencyMs}ms"
            : string.Empty;
    }
}
