using System.Windows;

namespace Makara.Desktop.Services;

/// <summary>
/// 主题服务：运行时切换暗色 / 浅色主题（替换 App 合并字典中的主题资源）
/// </summary>
public class ThemeService
{
    public const string Dark = "dark";
    public const string Light = "light";

    public string Current { get; private set; } = Dark;

    public event Action<string>? ThemeChanged;

    /// <summary>应用主题（dark / light），不区分大小写</summary>
    public void Apply(string theme)
    {
        var normalized = theme?.Trim().ToLowerInvariant() switch
        {
            Light => Light,
            _ => Dark
        };
        if (normalized == Current) return;

        Current = normalized;
        var uri = new Uri($"Themes/Theme.{(Current == Dark ? "Dark" : "Light")}.xaml", UriKind.Relative);
        var dict = (ResourceDictionary)Application.LoadComponent(uri);
        Application.Current.Resources.MergedDictionaries[0] = dict;
        ThemeChanged?.Invoke(Current);
    }

    public void Toggle() => Apply(Current == Dark ? Light : Dark);
}
