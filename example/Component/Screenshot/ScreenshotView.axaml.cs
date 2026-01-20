using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using example.State;

namespace example.Component.Screenshot;

public partial class ScreenshotView : Window{
    public ScreenshotView(){
        ShowInTaskbar = false;
        InitializeComponent();

    }
    public Rect? Result { get; set; }
    private Point? _startPoint;
    private bool _isSelecting = false;

    private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e){
        if (e.GetCurrentPoint(MainCanvas).Properties.IsLeftButtonPressed){
            _startPoint = e.GetPosition(MainCanvas);
            _isSelecting = true;
            SelectionRectangle.IsVisible = IsVisible;
            UpdateSelectionRectangle(_startPoint.Value, _startPoint.Value);
        }
    }

    private void Canvas_PointerMoved(object? sender, PointerEventArgs e){
        if (_isSelecting && _startPoint.HasValue){
            var currentPoint = e.GetPosition(MainCanvas);
            UpdateSelectionRectangle(_startPoint.Value, currentPoint);
        }
    }

    private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e){
        if (_isSelecting && e.InitialPressMouseButton == MouseButton.Left){
            _isSelecting = false;
            SelectionRectangle.IsVisible = false;

            // 可选：获取最终选中区域
            var rect = GetSelectionRect(_startPoint!.Value, e.GetPosition(MainCanvas));
            // 这里可以处理选中逻辑，比如选中 Canvas 中的其他控件
            Logger.Log($"Selected area: {rect}");
            
            var end = e.GetPosition(this);
            Result = GetSelectionRect(_startPoint!.Value, end);
        }
    }

    private void UpdateSelectionRectangle(Point start, Point end){
        var rect = GetSelectionRect(start, end);
        Canvas.SetLeft(SelectionRectangle, rect.X);
        Canvas.SetTop(SelectionRectangle, rect.Y);
        SelectionRectangle.Width = rect.Width;
        SelectionRectangle.Height = rect.Height;
    }

    private Rect GetSelectionRect(Point p1, Point p2){
        var x = Math.Min(p1.X, p2.X);
        var y = Math.Min(p1.Y, p2.Y);
        var width = Math.Abs(p1.X - p2.X);
        var height = Math.Abs(p1.Y - p2.Y);
        return new Rect(x, y, width, height);
    }
}