using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BoxModel = NewDesktop.ViewModels.BoxModel;

namespace NewDesktop.Views;

public partial class BoxView
{
    private bool _isExpanded = true;       // 当前是否展开状态

    private readonly Thumb[] _resizeThumbs;
    
    /// <summary>
    /// 边缘空白区域宽度（用于计算有效内容区域）
    /// </summary>
    private static double MARGIN_SIZE { get; } = 7;
    
    /// <summary>
    /// 尺寸对齐单位（按住CTRL时调整尺寸的基准单位）
    /// </summary>
    public static double SNAP_UNIT { get; set; } = 64;

    public BoxView()
    {
        
        InitializeComponent();

        // ss();
        // 初始化调整手柄数组（按顺序存储八个方向的手柄）
        _resizeThumbs = [Resize_L, Resize_R, Resize_T, Resize_B, Resize_T_L, Resize_T_R, Resize_B_L, Resize_B_R];InitializeResizeHandlers();
    }

    // private void ss()
    // {
    //     if (!(DataContext is BoxModel iconData)) return;
    //     _expandedHeight = iconData.Height;
    // }

    // 折叠/展开按钮点击事件处理
    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (!(DataContext is BoxModel iconData)) return;
        
        if (_isExpanded)
        {
            // 禁用动画期间的手柄操作
            foreach (var thumb in _resizeThumbs)
            {
                thumb.IsEnabled = false;
            }
        
            // 动画：收缩到仅标题栏高度
            var animation = new DoubleAnimation
            {
                To = iconData.HeadHeight,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            animation.Completed += (s, args) =>
            {
                BeginAnimation(HeightProperty, null);
                // 动画结束后更新ViewModel
                iconData.Height1 = iconData.HeadHeight;
                _isExpanded = !_isExpanded;
                Debug.WriteLine($"收起: {iconData.Height}");
                // RestoreThumbs();
            };
            BeginAnimation(HeightProperty, animation);
            return;
        }

        if (!_isExpanded)
        {
            // 动画：展开到保存的高度
            var animation1 = new DoubleAnimation
            {
                To = iconData.Height,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
        
            animation1.Completed += (s, args) =>
            {
                BeginAnimation(HeightProperty, null);
                // 动画结束后更新ViewModel
                iconData.Height1 = iconData.Height;
                _isExpanded = !_isExpanded;
                Debug.WriteLine($"展开: {iconData.Height}");
                // 禁用动画期间的手柄操作
                foreach (var thumb in _resizeThumbs)
                {
                    thumb.IsEnabled = true;
                }
                // RestoreThumbs();
            };
       
            BeginAnimation(HeightProperty, animation1);
            return;
        }
        // if (_isExpanded)
        // {
        //     // 保存实际内容高度（排除标题栏）
        //     _expandedHeight = iconData.Height;
        //
        //     // 直接设置高度（不通过动画）
        //     iconData.Height = 24;
        //     // ContentScroll.Visibility = Visibility.Collapsed;
        //     foreach (var thumb in _resizeThumbs)
        //     {
        //         thumb.IsEnabled = false;
        //     }
        // }
        // else
        // {
        //     // 恢复高度并显示内容
        //     iconData.Height = _expandedHeight;
        //     // ContentScroll.Visibility = Visibility.Visible;
        //     foreach (var thumb in _resizeThumbs)
        //     {
        //         thumb.IsEnabled = true;
        //     }
        // }
        
        Debug.WriteLine($"现在: {iconData.Height}");
    }

    #region 尺寸调整功能

    /// <summary>
    /// 初始化所有调整手柄的拖动事件
    /// </summary>
    private void InitializeResizeHandlers()
    {
        // 四边调整配置
        _resizeThumbs[0].DragDelta += (s, e) => AdjustWidth(-e.HorizontalChange, true);  // 左侧调整
        _resizeThumbs[1].DragDelta += (s, e) => AdjustWidth(e.HorizontalChange, false);  // 右侧调整
        _resizeThumbs[2].DragDelta += (s, e) => AdjustHeight(-e.VerticalChange, true);   // 顶部调整
        _resizeThumbs[3].DragDelta += (s, e) => AdjustHeight(e.VerticalChange, false);    // 底部调整

        // 四角调整配置（组合宽度和高度调整）
        _resizeThumbs[4].DragDelta += (s, e) => { AdjustWidth(-e.HorizontalChange, true); AdjustHeight(-e.VerticalChange, true); };  // 左上角
        _resizeThumbs[5].DragDelta += (s, e) => { AdjustWidth(e.HorizontalChange, false); AdjustHeight(-e.VerticalChange, true); };  // 右上角
        _resizeThumbs[6].DragDelta += (s, e) => { AdjustWidth(-e.HorizontalChange, true); AdjustHeight(e.VerticalChange, false); };  // 左下角
        _resizeThumbs[7].DragDelta += (s, e) => { AdjustWidth(e.HorizontalChange, false); AdjustHeight(e.VerticalChange, false); }; // 右下角
    }

    /// <summary>
    /// 调整控件宽度
    /// </summary>
    private void AdjustWidth(double delta, bool adjustLeft)
    {
        if (!(DataContext is BoxModel BoxModel))
            return;

        // 计算原始新宽度（当前宽度加上变化量）
        var rawWidth = Width + delta;

        // 计算边距总宽度（左右边距之和）
        double marginTotal = MARGIN_SIZE * 2;

        // 应用尺寸约束（考虑Ctrl键的吸附效果）
        var newWidth = ApplySizeConstraint(rawWidth, SNAP_UNIT, marginTotal);

        // 确保不小于最小尺寸（基准单位+边距）
        newWidth = Math.Max(newWidth, SNAP_UNIT + marginTotal);

        // 计算实际宽度变化量
        var widthDelta = newWidth - Width;

        // 调整左侧位置（当从左侧调整时）
        if (adjustLeft)
        {
            var newX = BoxModel.X - widthDelta;
            BoxModel.X = newX;
        }

        // 应用新宽度
        BoxModel.Width = newWidth;
    }

    /// <summary>
    /// 调整控件高度
    /// </summary>
    private void AdjustHeight(double delta, bool adjustTop)
    {
        if (!(DataContext is BoxModel BoxModel))
            return;

        // 计算原始新高度（当前高度加上变化量）
        var rawHeight = BoxModel.Height + delta;

        // 计算边距总高度（顶部边距+标题栏高度）
        double marginTotal = MARGIN_SIZE + BoxModel.HeadHeight;

        // 应用尺寸约束
        var newHeight = ApplySizeConstraint(rawHeight, SNAP_UNIT, marginTotal);

        // 确保不小于最小尺寸
        newHeight = Math.Max(newHeight, SNAP_UNIT + marginTotal);

        // 计算实际高度变化量

        // 调整顶部位置
        if (adjustTop)
        {
            BoxModel.Y -= (newHeight - BoxModel.Height);
        }

        // 应用新高度
        BoxModel.Height = newHeight;
        // Height = newHeight;
        // _expandedHeight = newHeight; // 保持缓存值更新
        // 使用字符串插值输出变量值
        Debug.WriteLine($"newHeight: {newHeight}");
    }

    /// <summary>
    /// 应用尺寸约束逻辑（当按住Ctrl键时对齐到基准单位）
    /// </summary>
    private double ApplySizeConstraint(double rawSize, double unit, double margin)
    {
        // 检查Ctrl键状态
        if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            return rawSize;

        // 计算对齐后的尺寸（四舍五入到最近的单位倍数）
        return Math.Round((rawSize - margin) / unit) * unit + margin;
    }
    #endregion
}