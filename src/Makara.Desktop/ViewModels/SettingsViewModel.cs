using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

/// <summary>
/// 设置页 ViewModel：外观 / 默认参数 / 数据安全 / 通知 / 关于
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly ThemeService _theme;

    /// <summary>设置模型（直接双向绑定）</summary>
    public AppSettings S => _settings.Settings;

    [ObservableProperty] private string _statusMessage = string.Empty;

    public SettingsViewModel(SettingsService settings, ThemeService theme)
    {
        _settings = settings;
        _theme = theme;
    }

    [RelayCommand]
    private void SetTheme(string theme)
    {
        _theme.Apply(theme);
        _settings.Settings.Theme = _theme.Current;
        _settings.Save();
        OnPropertyChanged(nameof(S));
        StatusMessage = "主题已切换";
    }

    [RelayCommand]
    private void SetScale(string scale)
    {
        _settings.Settings.UiScale = scale;
        _settings.Save();
        OnPropertyChanged(nameof(S));
        StatusMessage = "界面缩放已保存（重启客户端后完全生效）";
    }

    [RelayCommand]
    private void Save()
    {
        _settings.Save();
        StatusMessage = "设置已保存";
    }

    [RelayCommand]
    private void Reset()
    {
        _settings.Reset();
        _theme.Apply(_settings.Settings.Theme);
        _settings.Save();
        OnPropertyChanged(nameof(S));
        StatusMessage = "已恢复默认设置";
    }

    [RelayCommand]
    private void CheckUpdate()
    {
        StatusMessage = "当前已是最新版本（v0.1.0 MVP）";
    }
}
