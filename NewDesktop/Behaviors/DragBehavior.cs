using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using BoxModel = NewDesktop.ViewModels.BoxModel;

namespace NewDesktop.Behaviors;

/// <summary>
/// 实现拖放功能的交互行为，支持在Canvas内拖拽元素和边缘吸附
/// </summary>
public class DragBehavior : Behavior<FrameworkElement>
{
    private Point _startPoint;
    private bool _isDragging;
    private const double SnapThreshold = 10.0; // 吸附阈值
    
    #region 事件绑定
    protected override void OnAttached()
    {
        AssociatedObject.MouseLeftButtonDown += OnMouseDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseUp;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
    }
    #endregion

    #region 拖拽处理
    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (FindParentCanvas() is not { } canvas) return;
        
        _isDragging = true;
        _startPoint = e.GetPosition(canvas);
        AssociatedObject.CaptureMouse();
        e.Handled = true;
    }
    
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || FindParentCanvas() is not { } canvas) return;

        var currentPoint = e.GetPosition(canvas);
        var delta = currentPoint - _startPoint;

        if (AssociatedObject.DataContext is BoxModel item)
        {
            double originalX = item.X;
            double originalY = item.Y;

            // 应用位移
            item.X += delta.X;
            item.Y += delta.Y;

            // 执行边缘吸附
            SnapToEdges(item, canvas);

            // 计算吸附导致的位移补偿
            var adjustedDelta = new Point(
                item.X - (originalX + delta.X),
                item.Y - (originalY + delta.Y));

            // 调整起始点以保持拖动连贯性
            _startPoint = currentPoint + (Vector)adjustedDelta;
        }
        else
        {
            _startPoint = currentPoint;
        }
        
        e.Handled = true;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        AssociatedObject.ReleaseMouseCapture();
    }
    #endregion
    
    #region 工具
    /// <summary>
    /// 边缘吸附逻辑（画布边界+其他元素）
    /// </summary>
    private void SnapToEdges(BoxModel currentItem, Canvas canvas)
    {
        double elementWidth = AssociatedObject.ActualWidth;
        double elementHeight = AssociatedObject.ActualHeight;

        // 1. 画布边界吸附
        SnapToCanvas(currentItem, canvas, elementWidth, elementHeight);
        
        // 2. 其他元素吸附
        foreach (var child in canvas.Children)
        {
            if (ShouldSnapToElement(child, out var otherItem, out var otherWidth, out var otherHeight))
            {
                CheckHorizontalSnap(currentItem, otherItem, elementWidth, otherWidth);
                CheckVerticalSnap(currentItem, otherItem, elementHeight, otherHeight);
            }
        }
    }

    /// <summary>
    /// 画布边界吸附判断
    /// </summary>
    private void SnapToCanvas(BoxModel item, Canvas canvas, double width, double height)
    {
        // 左边界
        if (item.X < SnapThreshold) item.X = 0;
        // 右边界
        else if (item.X + width > canvas.ActualWidth - SnapThreshold) 
            item.X = canvas.ActualWidth - width;

        // 上边界
        if (item.Y < SnapThreshold) item.Y = 0;
        // 下边界
        else if (item.Y + height > canvas.ActualHeight - SnapThreshold) 
            item.Y = canvas.ActualHeight - height;
    }

    /// <summary>
    /// 判断是否为可吸附元素
    /// </summary>
    private bool ShouldSnapToElement(object child, out BoxModel otherItem, out double otherWidth, out double otherHeight)
    {
        otherItem = null;
        otherWidth = otherHeight = 0;

        if (child is not FrameworkElement element || 
            element == AssociatedObject || 
            element.DataContext is not BoxModel item) 
            return false;

        otherItem = item;
        otherWidth = element.ActualWidth;
        otherHeight = element.ActualHeight;
        return true;
    }

    /// <summary>
    /// 水平方向吸附检测
    /// </summary>
    private void CheckHorizontalSnap(BoxModel current, BoxModel other, double currentWidth, double otherWidth)
    {
        double currentRight = current.X + currentWidth;
        double otherRight = other.X + otherWidth;

        // 当前元素的右边缘贴附到目标的左边缘
        if (Math.Abs(currentRight - other.X) <= SnapThreshold)
            current.X = other.X - currentWidth;
        
        // 当前元素的左边缘贴附到目标的右边缘
        else if (Math.Abs(current.X - otherRight) <= SnapThreshold)
            current.X = otherRight;
    }

    /// <summary>
    /// 垂直方向吸附检测
    /// </summary>
    private void CheckVerticalSnap(BoxModel current, BoxModel other, double currentHeight, double otherHeight)
    {
        double currentBottom = current.Y + currentHeight;
        double otherBottom = other.Y + otherHeight;

        // 当前元素的下边缘贴附到目标的上边缘
        if (Math.Abs(currentBottom - other.Y) <= SnapThreshold)
            current.Y = other.Y - currentHeight;
        
        // 当前元素的上边缘贴附到目标的下边缘
        else if (Math.Abs(current.Y - otherBottom) <= SnapThreshold)
            current.Y = otherBottom;
    }

    /// <summary>
    /// 查找父级Canvas容器
    /// </summary>
    private Canvas? FindParentCanvas(DependencyObject? current = null)
    {
        current ??= AssociatedObject;
        while (current != null)
        {
            if (current is Canvas canvas) return canvas;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }


    // /// <summary>查找控件（ItemsControl或Canvas）</summary>
    // /// <param name="startObject">查找起点</param>
    // /// <param name="typeObject">查找目标</param>
    // /// <returns>查找到的目标，没有时为空</returns>
    // private FrameworkElement? FindContainer(DependencyObject? startObject, List<Type> typeObject)
    // {
    //     while (startObject != null)
    //     {
    //         if (typeObject.Any(t => t.IsInstanceOfType(startObject)))
    //         {
    //             return (FrameworkElement)startObject;
    //         }
    //         startObject = VisualTreeHelper.GetParent(startObject);
    //     }
    //     return null;
    // }
    #endregion
}