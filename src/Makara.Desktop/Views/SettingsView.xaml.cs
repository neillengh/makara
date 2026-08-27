using System.Diagnostics;
using System.Windows.Controls;

namespace Makara.Desktop.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void OnProjectLinkClick(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/neillengh/makara",
                UseShellExecute = true
            });
        }
        catch
        {
            // 打开浏览器失败时忽略
        }
    }
}
