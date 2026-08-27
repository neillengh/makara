using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Desktop.Services;
using Makara.Desktop.Views;

namespace Makara.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly SseClient _sse;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentPage = "workflows";

    public MainViewModel(ApiClient api, SseClient sse)
    {
        _api = api;
        _sse = sse;
        Navigate("workflows");
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        CurrentPage = page;
        CurrentView = page switch
        {
            "workflows" => new WorkflowsView
            {
                DataContext = new WorkflowsViewModel(_api) { OnEditWorkflow = OpenCanvas }
            },
            "datasources" => new DataSourcesView { DataContext = new DataSourcesViewModel(_api) },
            "runs" => new RunsView { DataContext = new RunsViewModel(_sse) },
            _ => new TextBlock
            {
                Text = "敬请期待",
                FontSize = 18,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xA0, 0xA0, 0xB0))
            }
        };
    }

    private void OpenCanvas(string workflowId)
    {
        CurrentPage = "canvas";
        CurrentView = new WorkflowCanvasView
        {
            DataContext = new WorkflowCanvasViewModel(_api, workflowId)
        };
    }
}
