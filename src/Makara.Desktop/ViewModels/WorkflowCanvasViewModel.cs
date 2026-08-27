using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Makara.Core.Models;
using Makara.Desktop.Services;

namespace Makara.Desktop.ViewModels;

public partial class WorkflowCanvasViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private Workflow? _workflow;

    public ObservableCollection<NodeItemViewModel> CanvasNodes { get; } = [];
    public ObservableCollection<EdgeItemViewModel> CanvasEdges { get; } = [];

    [ObservableProperty]
    private string _workflowId = string.Empty;

    [ObservableProperty]
    private string _workflowName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private NodeItemViewModel? _selectedNode;

    [ObservableProperty]
    private EdgeItemViewModel? _selectedEdge;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private Geometry? _tempConnectionPath;

    private NodeItemViewModel? _connectingSource;

    public WorkflowCanvasViewModel(ApiClient api, string workflowId)
    {
        _api = api;
        _workflowId = workflowId;
    }

    public WorkflowCanvasViewModel(ApiClient api)
    {
        _api = api;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(WorkflowId))
        {
            StatusMessage = "未指定工作流";
            return;
        }

        IsLoading = true;
        StatusMessage = "正在加载...";
        try
        {
            var wf = await _api.GetWorkflowAsync(WorkflowId);
            if (wf == null)
            {
                StatusMessage = "工作流不存在";
                return;
            }

            _workflow = wf;
            WorkflowName = wf.Name;
            CanvasNodes.Clear();
            CanvasEdges.Clear();

            var nodeMap = new Dictionary<string, NodeItemViewModel>();
            foreach (var n in wf.Nodes)
            {
                var nvm = new NodeItemViewModel(n);
                nodeMap[n.Id] = nvm;
                CanvasNodes.Add(nvm);
            }

            foreach (var e in wf.Edges)
            {
                var evm = new EdgeItemViewModel(e);
                if (nodeMap.TryGetValue(e.SourceNodeId, out var src) &&
                    nodeMap.TryGetValue(e.TargetNodeId, out var tgt))
                {
                    evm.UpdatePath(src, tgt);
                }
                CanvasEdges.Add(evm);
            }

            StatusMessage = $"已加载 {CanvasNodes.Count} 个节点、{CanvasEdges.Count} 条连接";
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

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_workflow == null)
        {
            StatusMessage = "无工作流可保存";
            return;
        }

        IsLoading = true;
        StatusMessage = "正在保存...";

        _workflow.Nodes.Clear();
        _workflow.Edges.Clear();

        foreach (var nvm in CanvasNodes)
        {
            nvm.SyncBack();
            _workflow.Nodes.Add(nvm.ToModel());
        }

        foreach (var evm in CanvasEdges)
        {
            _workflow.Edges.Add(evm.ToModel());
        }

        _workflow.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _api.UpdateWorkflowAsync(_workflow.Id, _workflow);
            StatusMessage = "保存成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        if (_workflow == null) return;

        await SaveAsync();

        IsLoading = true;
        StatusMessage = "正在触发...";
        try
        {
            var runId = await _api.RunWorkflowAsync(_workflow.Id);
            StatusMessage = $"已触发，运行 ID: {runId}";
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

    public void AddNode(string type, double x, double y)
    {
        var node = new WorkflowNode
        {
            Type = type,
            Label = NodeItemViewModel.GetDefaultLabel(type),
            X = x,
            Y = y
        };
        var nvm = new NodeItemViewModel(node);
        CanvasNodes.Add(nvm);
        SelectedNode = nvm;
        StatusMessage = $"已添加节点: {nvm.Label}";
    }

    [RelayCommand]
    private void DeleteSelectedNode()
    {
        if (SelectedNode == null) return;

        var nodeId = SelectedNode.Id;
        for (var i = CanvasEdges.Count - 1; i >= 0; i--)
        {
            if (CanvasEdges[i].SourceNodeId == nodeId || CanvasEdges[i].TargetNodeId == nodeId)
                CanvasEdges.RemoveAt(i);
        }
        CanvasNodes.Remove(SelectedNode);
        SelectedNode = null;
        StatusMessage = "节点已删除";
    }

    public void DeleteNode(NodeItemViewModel node)
    {
        var nodeId = node.Id;
        for (var i = CanvasEdges.Count - 1; i >= 0; i--)
        {
            if (CanvasEdges[i].SourceNodeId == nodeId || CanvasEdges[i].TargetNodeId == nodeId)
                CanvasEdges.RemoveAt(i);
        }
        CanvasNodes.Remove(node);
        if (SelectedNode == node)
            SelectedNode = null;
        StatusMessage = "节点已删除";
    }

    public void DeleteEdge(EdgeItemViewModel edge)
    {
        CanvasEdges.Remove(edge);
        if (SelectedEdge == edge)
            SelectedEdge = null;
        StatusMessage = "连接已删除";
    }

    public void SelectNode(NodeItemViewModel? node)
    {
        foreach (var n in CanvasNodes)
            n.IsSelected = n == node;
        SelectedNode = node;
        SelectedEdge = null;
    }

    public void SelectEdge(EdgeItemViewModel? edge)
    {
        foreach (var e in CanvasEdges)
            e.IsSelected = e == edge;
        SelectedEdge = edge;
        SelectedNode = null;
    }

    public void StartConnection(NodeItemViewModel source)
    {
        IsConnecting = true;
        _connectingSource = source;
        source.IsConnecting = true;
        StatusMessage = $"正在从 {source.Label} 创建连接，点击目标节点的输入端口";
    }

    public bool TryCompleteConnection(NodeItemViewModel target)
    {
        if (!IsConnecting || _connectingSource == null || _connectingSource == target)
        {
            CancelConnection();
            return false;
        }

        if (CanvasEdges.Any(e => e.SourceNodeId == _connectingSource.Id && e.TargetNodeId == target.Id))
        {
            StatusMessage = "连接已存在";
            CancelConnection();
            return false;
        }

        var edge = new WorkflowEdge
        {
            SourceNodeId = _connectingSource.Id,
            TargetNodeId = target.Id
        };
        var evm = new EdgeItemViewModel(edge);
        evm.UpdatePath(_connectingSource, target);
        CanvasEdges.Add(evm);

        StatusMessage = $"已连接: {_connectingSource.Label} → {target.Label}";
        CancelConnection();
        return true;
    }

    public void CancelConnection()
    {
        if (_connectingSource != null)
            _connectingSource.IsConnecting = false;
        IsConnecting = false;
        _connectingSource = null;
        TempConnectionPath = null;
    }

    public void UpdateTempConnection(double mouseX, double mouseY)
    {
        if (!IsConnecting || _connectingSource == null) return;

        var sx = _connectingSource.OutputPortX;
        var sy = _connectingSource.OutputPortY;
        var offset = System.Math.Max(40, System.Math.Abs(mouseX - sx) * 0.3);
        var data = $"M {sx},{sy} C {sx + offset},{sy} {mouseX - offset},{mouseY} {mouseX},{mouseY}";
        TempConnectionPath = Geometry.Parse(data);
    }

    public void RefreshEdgePaths()
    {
        var nodeMap = CanvasNodes.ToDictionary(n => n.Id);
        foreach (var evm in CanvasEdges)
        {
            if (nodeMap.TryGetValue(evm.SourceNodeId, out var src) &&
                nodeMap.TryGetValue(evm.TargetNodeId, out var tgt))
            {
                evm.UpdatePath(src, tgt);
            }
        }
    }
}
