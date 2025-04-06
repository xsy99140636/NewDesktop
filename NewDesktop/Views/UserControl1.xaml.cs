using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NewDesktop.ViewModels;

namespace NewDesktop.Views;

/// <summary>
/// UserControl1.xaml 的交互逻辑
/// </summary>
public partial class UserControl1
{
    
    private bool _isExpanded = true;       // 当前是否展开状态
    private double _expandedHeight;        // 保存展开时的控件总高度
    
    /// <summary>
    /// 边缘空白区域宽度（用于计算有效内容区域）
    /// </summary>
    private static double MARGIN_SIZE { get; } = 7;

    /// <summary>
    /// 标题栏标准高度（包含折叠按钮区域）
    /// </summary>
    public static double Header_SIZE { get; } = 24;
    
    /// <summary>
    /// 尺寸对齐单位（按住CTRL时调整尺寸的基准单位）
    /// </summary>
    public static double SNAP_UNIT { get; set; } = 64;
    public UserControl1()
    {
        InitializeComponent();
        // 订阅Loaded事件，在控件加载完成后执行初始化
        Loaded += OnLoaded;
        InitializeResizeHandlers();
    }

    // 控件加载完成事件处理
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 保存控件的初始高度作为展开时的高度
        // ActualHeight是控件的实际渲染高度
        _expandedHeight = this.ActualHeight;
    }

    // 折叠/展开按钮点击事件处理
    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (!(DataContext is BoxModel iconData)) return;
        
        if (_isExpanded)
        {
            // 当前是展开状态，执行折叠操作

            // 创建高度动画：从当前高度缩放到24像素(只保留标题栏)
            var animation = new DoubleAnimation
            {
                To = 24,  // 目标高度(24像素)
                Duration = TimeSpan.FromSeconds(0.3),  // 动画持续时间0.3秒
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                // 缓动函数使动画更自然(先快后慢)
            };

            // 将动画应用到控件的Height属性
            BeginAnimation(HeightProperty, animation);

        }
        else
        {
            // 当前是折叠状态，执行展开操作

            // 创建高度动画：从当前高度(24像素)恢复到之前保存的展开高度
            var animation = new DoubleAnimation
            {
                To = iconData.Height,  // 目标高度(之前保存的展开高度)
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // 将动画应用到控件的Height属性
            BeginAnimation(HeightProperty, animation);

        }

        // 切换展开/折叠状态标志
        _isExpanded = !_isExpanded;
    }

    #region 尺寸调整功能

    /// <summary>
    /// 初始化所有调整手柄的拖动事件
    /// </summary>
    private void InitializeResizeHandlers()
    {
        // 初始化调整手柄数组（按顺序存储八个方向的手柄）
        Thumb[] resizeThumbs = new[] { Resize_L, Resize_R, Resize_T, Resize_B, Resize_T_L, Resize_T_R, Resize_B_L, Resize_B_R };
        
        // 四边调整配置
        resizeThumbs[0].DragDelta += (s, e) => AdjustWidth(-e.HorizontalChange, true);  // 左侧调整
        resizeThumbs[1].DragDelta += (s, e) => AdjustWidth(e.HorizontalChange, false);  // 右侧调整
        resizeThumbs[2].DragDelta += (s, e) => AdjustHeight(-e.VerticalChange, true);   // 顶部调整
        resizeThumbs[3].DragDelta += (s, e) => AdjustHeight(e.VerticalChange, false);    // 底部调整

        // 四角调整配置（组合宽度和高度调整）
        resizeThumbs[4].DragDelta += (s, e) => { AdjustWidth(-e.HorizontalChange, true); AdjustHeight(-e.VerticalChange, true); };  // 左上角
        resizeThumbs[5].DragDelta += (s, e) => { AdjustWidth(e.HorizontalChange, false); AdjustHeight(-e.VerticalChange, true); };  // 右上角
        resizeThumbs[6].DragDelta += (s, e) => { AdjustWidth(-e.HorizontalChange, true); AdjustHeight(e.VerticalChange, false); };  // 左下角
        resizeThumbs[7].DragDelta += (s, e) => { AdjustWidth(e.HorizontalChange, false); AdjustHeight(e.VerticalChange, false); }; // 右下角
    }

    /// <summary>
    /// 调整控件宽度
    /// </summary>
    private void AdjustWidth(double delta, bool adjustLeft)
    {
        if (!(DataContext is BoxModel iconData)) return;

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
            var newX = iconData.X - widthDelta;
            iconData.X = newX;
        }

        // 应用新宽度
        iconData.Width = newWidth;
    }

    /// <summary>
    /// 调整控件高度
    /// </summary>
    private void AdjustHeight(double delta, bool adjustTop)
    {
        if (!(DataContext is BoxModel iconData)) return;

        // 计算原始新高度（当前高度加上变化量）
        var rawHeight = Height + delta;

        // 计算边距总高度（顶部边距+标题栏高度）
        double marginTotal = MARGIN_SIZE + Header_SIZE;

        // 应用尺寸约束
        var newHeight = ApplySizeConstraint(rawHeight, SNAP_UNIT, marginTotal);

        // 确保不小于最小尺寸
        newHeight = Math.Max(newHeight, SNAP_UNIT + marginTotal);

        // 计算实际高度变化量
        var heightDelta = newHeight - Height;

        // 调整顶部位置
        if (adjustTop)
        {
            var newY = iconData.Y - heightDelta;
            iconData.Y = newY;
        }

        // 应用新高度
        iconData.Height = newHeight;
    }

    /// <summary>
    /// 应用尺寸约束逻辑（当按住Ctrl键时对齐到基准单位）
    /// </summary>
    private double ApplySizeConstraint(double rawSize, double unit, double margin)
    {
        // 检查Ctrl键状态
        if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) return rawSize;

        // 计算对齐后的尺寸（四舍五入到最近的单位倍数）
        return Math.Round((rawSize - margin) / unit) * unit + margin;
    }
    #endregion
}
