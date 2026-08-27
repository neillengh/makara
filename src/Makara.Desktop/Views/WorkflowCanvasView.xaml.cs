using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using Makara.Desktop.Models;
using Makara.Desktop.ViewModels;

namespace Makara.Desktop.Views;

public partial class WorkflowCanvasView : UserControl
{
    private WorkflowCanvasViewModel? _vm;

    private bool _isDraggingNode;
    private Point _dragStart;
    private double _nodeStartX, _nodeStartY;
    private NodeItemViewModel? _draggingNode;

    public WorkflowCanvasView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _vm = DataContext as WorkflowCanvasViewModel;
        if (_vm != null && !string.IsNullOrEmpty(_vm.WorkflowId))
            _ = _vm.LoadCommand.ExecuteAsync(null);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window?.DataContext is MainViewModel mvm)
            mvm.NavigateCommand.Execute("workflows");
    }

    #region Palette → Canvas (Drag & Drop)

    private void PaletteItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is NodeTemplate template)
        {
            var data = new DataObject("NodeTemplate", template);
            DragDrop.DoDragDrop(fe, data, DragDropEffects.Copy);
            e.Handled = true;
        }
    }

    private void Canvas_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent("NodeTemplate")
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void Canvas_Drop(object sender, DragEventArgs e)
    {
        if (_vm == null) return;
        if (e.Data.GetData("NodeTemplate") is NodeTemplate template)
        {
            var pos = e.GetPosition(CanvasGrid);
            var x = pos.X - NodeItemViewModel.NodeWidth / 2;
            var y = pos.Y - NodeItemViewModel.NodeHeight / 2;
            _vm.AddNode(template.Type, Math.Max(0, x), Math.Max(0, y));
            e.Handled = true;
        }
    }

    #endregion

    #region Node Dragging

    private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm?.IsConnecting == true) return;

        if (sender is Border border && border.DataContext is NodeItemViewModel node)
        {
            _vm?.SelectNode(node);
            _draggingNode = node;
            _dragStart = e.GetPosition(CanvasGrid);
            _nodeStartX = node.X;
            _nodeStartY = node.Y;
            _isDraggingNode = false;
            border.CaptureMouse();
            e.Handled = true;
        }
    }

    private void Node_MouseMove(object sender, MouseEventArgs e)
    {
        if (_draggingNode == null || e.LeftButton != MouseButtonState.Pressed) return;

        var pos = e.GetPosition(CanvasGrid);
        var dx = pos.X - _dragStart.X;
        var dy = pos.Y - _dragStart.Y;

        if (!_isDraggingNode)
        {
            if (Math.Abs(dx) < 3 && Math.Abs(dy) < 3) return;
            _isDraggingNode = true;
        }

        _draggingNode.X = Math.Max(0, _nodeStartX + dx);
        _draggingNode.Y = Math.Max(0, _nodeStartY + dy);
        _vm?.RefreshEdgePaths();
    }

    private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.IsMouseCaptured)
            border.ReleaseMouseCapture();
        _draggingNode = null;
        _isDraggingNode = false;
    }

    #endregion

    #region Connection Drawing

    private void OutputPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Ellipse ellipse && ellipse.DataContext is NodeItemViewModel node && _vm != null)
        {
            _vm.StartConnection(node);
            e.Handled = true;
        }
    }

    private void InputPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Ellipse ellipse && ellipse.DataContext is NodeItemViewModel node && _vm != null)
        {
            _vm.TryCompleteConnection(node);
            e.Handled = true;
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_vm?.IsConnecting == true)
        {
            var pos = e.GetPosition(CanvasGrid);
            _vm.UpdateTempConnection(pos.X, pos.Y);
        }
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm?.IsConnecting == true)
        {
            _vm.CancelConnection();
        }
        else
        {
            _vm?.SelectNode(null);
            _vm?.SelectEdge(null);
        }
    }

    #endregion

    #region Edge Selection

    private void Edge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Path path && path.DataContext is EdgeItemViewModel edge && _vm != null)
        {
            _vm.SelectEdge(edge);
            e.Handled = true;
        }
    }

    #endregion

    #region Keyboard

    private void UserControl_KeyDown(object sender, KeyEventArgs e)
    {
        if (_vm == null) return;

        switch (e.Key)
        {
            case Key.Delete:
                if (_vm.SelectedNode != null)
                {
                    _vm.DeleteNode(_vm.SelectedNode);
                    e.Handled = true;
                }
                else if (_vm.SelectedEdge != null)
                {
                    _vm.DeleteEdge(_vm.SelectedEdge);
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                _vm.CancelConnection();
                _vm.SelectNode(null);
                _vm.SelectEdge(null);
                break;
        }
    }

    #endregion
}
