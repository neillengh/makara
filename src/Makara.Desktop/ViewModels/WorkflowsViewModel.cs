using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Core.Enums;
using Makara.Core.Models;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

public partial class WorkflowsViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly RunHistoryService? _runHistory;

    [ObservableProperty]
    private ObservableCollection<Workflow> _workflows = [];

    [ObservableProperty]
    private Workflow? _selectedWorkflow;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private string _newCronExpression = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _activeRunId = string.Empty;

    public Action<string>? OnEditWorkflow { get; set; }

    public WorkflowsViewModel(ApiClient api, RunHistoryService? runHistory = null)
    {
        _api = api;
        _runHistory = runHistory;
    }

    [RelayCommand]
    private void Edit(Workflow wf)
    {
        OnEditWorkflow?.Invoke(wf.Id);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        try
        {
            var list = await _api.GetWorkflowsAsync();
            Workflows.Clear();
            if (list != null)
                foreach (var wf in list)
                    Workflows.Add(wf);
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAsync()
    {
        var wf = new Workflow
        {
            Name = NewName,
            Description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription,
            CronExpression = string.IsNullOrWhiteSpace(NewCronExpression) ? null : NewCronExpression,
            Status = WorkflowStatus.Draft
        };

        IsLoading = true;
        StatusMessage = "正在创建...";
        try
        {
            var created = await _api.CreateWorkflowAsync(wf);
            if (created != null)
                Workflows.Add(created);
            StatusMessage = "创建成功";
            NewName = string.Empty;
            NewDescription = string.Empty;
            NewCronExpression = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"创建失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RunAsync(Workflow wf)
    {
        IsLoading = true;
        StatusMessage = $"正在触发工作流: {wf.Name}...";
        try
        {
            var runId = await _api.RunWorkflowAsync(wf.Id);
            ActiveRunId = runId;
            StatusMessage = $"已触发，运行ID: {runId}";
            _ = TrackRunAsync(runId, wf.Name);
        }
        catch (Exception ex)
        {
            StatusMessage = $"运行失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>后台轮询运行状态，写入本地运行历史（供仪表盘统计）</summary>
    private async Task TrackRunAsync(string runId, string workflowName)
    {
        if (_runHistory is null || string.IsNullOrEmpty(runId)) return;
        _runHistory.StartRun(runId, workflowName);
        try
        {
            for (var i = 0; i < 120; i++)
            {
                await Task.Delay(2000);
                var run = await _api.GetRunStatusAsync(runId);
                if (run is null) continue;
                var status = run.Status?.ToLowerInvariant() switch
                {
                    "completed" or "success" => "success",
                    "failed" or "error" => "failed",
                    "cancelled" or "canceled" => "cancelled",
                    _ => ""
                };
                if (status != "")
                {
                    _runHistory.FinishRun(runId, status);
                    return;
                }
            }
            _runHistory.FinishRun(runId, "failed");
        }
        catch
        {
            _runHistory.FinishRun(runId, "failed");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(Workflow wf)
    {
        try
        {
            await _api.DeleteWorkflowAsync(wf.Id);
            Workflows.Remove(wf);
            StatusMessage = "已删除";
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
    }

    private bool CanCreate() => !string.IsNullOrWhiteSpace(NewName);
}
