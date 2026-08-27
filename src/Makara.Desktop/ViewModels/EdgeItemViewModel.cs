using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Makara.Core.Models;

namespace Makara.Desktop.ViewModels;

public partial class EdgeItemViewModel : ObservableObject
{
    private readonly WorkflowEdge _edge;

    public string Id => _edge.Id;
    public string SourceNodeId => _edge.SourceNodeId;
    public string TargetNodeId => _edge.TargetNodeId;

    [ObservableProperty]
    private double _sourceX;

    [ObservableProperty]
    private double _sourceY;

    [ObservableProperty]
    private double _targetX;

    [ObservableProperty]
    private double _targetY;

    [ObservableProperty]
    private bool _isSelected;

    public WorkflowEdge ToModel() => _edge;

    public EdgeItemViewModel(WorkflowEdge edge)
    {
        _edge = edge;
    }

    public void UpdatePath(NodeItemViewModel source, NodeItemViewModel target)
    {
        SourceX = source.OutputPortX;
        SourceY = source.OutputPortY;
        TargetX = target.InputPortX;
        TargetY = target.InputPortY;
    }

    partial void OnSourceXChanged(double value) => OnPropertyChanged(nameof(PathData));
    partial void OnSourceYChanged(double value) => OnPropertyChanged(nameof(PathData));
    partial void OnTargetXChanged(double value) => OnPropertyChanged(nameof(PathData));
    partial void OnTargetYChanged(double value) => OnPropertyChanged(nameof(PathData));

    public Geometry PathData
    {
        get
        {
            var midX = (SourceX + TargetX) / 2;
            var offsetX = System.Math.Max(40, (TargetX - SourceX) * 0.3);
            var data = $"M {SourceX},{SourceY} C {SourceX + offsetX},{SourceY} {TargetX - offsetX},{TargetY} {TargetX},{TargetY}";
            return Geometry.Parse(data);
        }
    }
}
