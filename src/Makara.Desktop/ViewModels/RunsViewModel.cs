using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Core.Models;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

public partial class RunsViewModel : ObservableObject
{
    private readonly SseClient _sse;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubscribeCommand))]
    private string _runId = string.Empty;

    [ObservableProperty]
    private ObservableCollection<WorkflowEvent> _events = [];

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _currentNode = string.Empty;

    [ObservableProperty]
    private string _status = "等待订阅";

    [ObservableProperty]
    private bool _isRunning;

    public RunsViewModel(SseClient sse)
    {
        _sse = sse;
        _sse.OnEvent += OnSseEvent;
    }

    private void OnSseEvent(WorkflowEvent evt)
    {
        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            Events.Insert(0, evt);
            Progress = evt.Progress;
            if (!string.IsNullOrEmpty(evt.NodeId))
                CurrentNode = evt.NodeId;
            if (!string.IsNullOrEmpty(evt.Message))
                Status = evt.Message;

            if (evt.Type is "completed" or "error" or "cancelled")
                IsRunning = false;
        });
    }

    [RelayCommand(CanExecute = nameof(CanSubscribe))]
    private async Task SubscribeAsync()
    {
        Events.Clear();
        Progress = 0;
        CurrentNode = string.Empty;
        IsRunning = true;
        Status = "已连接，等待事件...";

        try
        {
            await _sse.SubscribeAsync(RunId);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Status = $"连接错误: {ex.Message}";
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void Unsubscribe()
    {
        _sse.Unsubscribe();
        IsRunning = false;
        Status = "已断开";
    }

    private bool CanSubscribe() => !string.IsNullOrWhiteSpace(RunId) && !IsRunning;
}
