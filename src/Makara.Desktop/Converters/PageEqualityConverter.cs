using System.Globalization;
using System.Windows.Data;

namespace Makara.Desktop.Converters;

/// <summary>
/// 当前页 key 相等性转换器（导航按钮 IsChecked 绑定用）
/// </summary>
public class PageEqualityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string page && parameter is string key && page == key;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
