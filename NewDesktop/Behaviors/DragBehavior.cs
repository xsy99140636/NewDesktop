using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using BoxModel = NewDesktop.ViewModels.BoxModel;

//using System.Windows.Input;
//using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
//using Point = System.Windows.Point;


namespace NewDesktop.Behaviors;

/// <summary>
/// 实现拖放功能的交互行为，支持在Canvas和ItemsControl容器之间拖拽元素
/// </summary>
public class DragBehavior : Behavior<FrameworkElement>
{
    // 拖动状态相关字段
    // private ContentPresenter _contentPresenter;
    // private Canvas _parentCanvas;
    // private Point _dragStartPosition;// 拖动起始位置（相对容器坐标系）
    //
    // private double _initialLeft, _initialTop;
    // private FrameworkElement? _parent;     // 父级画布容器引用
    
    // 拖拽起始点坐标（相对于Canvas）
    private Point _startPoint;
    // 拖拽状态标志
    private bool _isDragging;// 是否处于拖动状态标志位

    
    #region 事件绑定
    /// <summary>
    /// 将行为附加到UI元素时初始化事件监听
    /// </summary>
    protected override void OnAttached()
    {
        // _parent = FindContainer(AssociatedObject, [typeof(Canvas)]);
        // if (_parent != null)
        {
            AssociatedObject.MouseLeftButtonDown += OnMouseDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonUp += OnMouseUp;
        }

        
    }
    
    /// <summary>
    /// 当行为从元素分离时解除事件绑定
    /// </summary>
    protected override void OnDetaching()
    {
        AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
    }
    
    /// <summary>
    /// 鼠标按下事件处理器 - 开始拖动操作
    /// </summary>
    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        // 获取父级Canvas容器
        if (FindParentCanvas() is not { } canvas) return;

        // 初始化拖拽状态
        _isDragging = true;
        _startPoint = e.GetPosition(canvas);  // 记录起始点（相对于Canvas坐标系）
        AssociatedObject.CaptureMouse();      // 捕获鼠标确保后续事件
        e.Handled = true;                     // 标记事件已处理
    }

    /// <summary>
    /// 鼠标移动事件处理器 - 处理拖动过程
    /// </summary>
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || FindParentCanvas() is not { } canvas) return;

        // 计算当前鼠标位置相对于Canvas的坐标
        var currentPoint = e.GetPosition(canvas);
        // 计算与起始点的位移差
        var delta = currentPoint - _startPoint;

        // 更新绑定的数据模型坐标（需DataContext为PositionedObject类型）
        if (AssociatedObject.DataContext is BoxModel item)
        {
            // 限制最小坐标为0，防止移出Canvas左/上边界
            // item.X = Math.Max(0, item.X + delta.X);
            // item.Y = Math.Max(0, item.Y + delta.Y);
            item.X += delta.X;
            item.Y += delta.Y;
        }

        _startPoint = currentPoint;  // 更新起始点为当前位置
    }

    /// <summary>
    /// 鼠标释放事件处理器 - 完成拖动操作
    /// </summary>
    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        // 重置拖拽状态标志
        _isDragging = false;
        
        // 释放鼠标捕获
        AssociatedObject.ReleaseMouseCapture();
    }
    #endregion

    #region 工具

    /// <summary>
    /// 在可视化树中查找最近的父级Canvas容器
    /// </summary>
    /// <param name="current">起始查找节点（默认当前关联对象）</param>
    /// <returns>找到的Canvas对象，未找到返回null</returns>
    private Canvas? FindParentCanvas(DependencyObject? current = null)
    {
        current ??= AssociatedObject;
        // 沿可视化树向上查找Canvas父容器
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