using System.Windows.Controls.Primitives;
using System.Windows.Input;
using BoxModel = NewDesktop.ViewModels.BoxModel;

namespace NewDesktop.Views;

public partial class BoxView
{
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
    
    public BoxView()
    {
        InitializeComponent();
        InitializeResizeHandlers();
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
        if (!(DataContext is BoxModel iconData))
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
        if (!(DataContext is BoxModel iconData))
            return;

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
        if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            return rawSize;

        // 计算对齐后的尺寸（四舍五入到最近的单位倍数）
        return Math.Round((rawSize - margin) / unit) * unit + margin;
    }
    #endregion
}