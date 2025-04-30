using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BoxModel = NewDesktop.ViewModels.BoxModel;

namespace NewDesktop.Views;

public partial class BoxView
{
    // 调整手柄数组
    private readonly Thumb[] _resizeThumbs;
    
    /// <summary>
    /// 边缘空白区域宽度（用于计算有效内容区域）
    /// </summary>
    private static double MARGIN_SIZE { get; } = 7;
    
    /// <summary>
    /// 尺寸对齐单位（按住CTRL时调整尺寸的基准单位）
    /// </summary>
    private static double SNAP_UNIT { get; set; } = 64;
    
    public BoxView()
    {
        InitializeComponent();
        // 初始化调整手柄数组（按顺序存储八个方向的手柄）
        _resizeThumbs = [Resize_L, Resize_R, Resize_T, Resize_B, Resize_T_L, Resize_T_R, Resize_B_L, Resize_B_R];
        InitializeResizeHandlers();
        Loaded += SettingsWindow_Loaded;
    }

    private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        CreateContextMenu();
    }

    #region 折叠/展开功能

    private double _to;
    
    private DoubleAnimation? _animation;

    private void HeadDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!(DataContext is BoxModel iconData)) return;

        if (iconData.IsExpanded == false)
        {
            iconData.IsExpanded = true;
            _to = iconData.Height;
            foreach (var thumb in _resizeThumbs) thumb.IsEnabled = false;
            CollapseToExpandTheAnimation();
        }
        else if (iconData.IsExpanded == true)
        {
            iconData.IsExpanded = false;
            _to = iconData.HeadHeight;
            CollapseToExpandTheAnimation();
        }

        //    Debug.WriteLine($"现在: {iconData.Height}");
    }

    private void MouseEnterBox(object sender, MouseEventArgs e)
    {
        if (!(DataContext is BoxModel iconData)) return;
        _to = iconData.Height;
        foreach (var thumb in _resizeThumbs) thumb.IsEnabled = false;
        if (iconData.IsExpanded == null) CollapseToExpandTheAnimation();
    }

    private void MouseLeaveBox(object sender, MouseEventArgs e)
    {
        if (!(DataContext is BoxModel iconData)) return;
        _to = iconData.HeadHeight;
        if (iconData.IsExpanded == null) CollapseToExpandTheAnimation();
    }

    private void CollapseToExpandTheAnimation()
    {
    if ( _animation != null) _animation.Completed -= AnimationCompletedHandler;
        
    //    // 动画：展开到保存的高度
    _animation = new DoubleAnimation
    {
        // From = ActualHeight,
        To = _to,
        Duration = TimeSpan.FromSeconds(0.2),
        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
    };
        
    _animation.Completed += AnimationCompletedHandler;
        
    BeginAnimation(HeightProperty, _animation);
    }

    private void AnimationCompletedHandler(object? sender, EventArgs e)
    {
        if (!(DataContext is BoxModel iconData)) return;

        BeginAnimation(HeightProperty, null);

        iconData.Height1 = _to;

        if (iconData.IsExpanded == true)
        {
            //        // 恢复手柄操作
            foreach (var thumb in _resizeThumbs) thumb.IsEnabled = true;
            //    }

            //    Debug.WriteLine($"完成: {iconData.Height}");
        }
    }

    #endregion
    
    #region 尺寸调整功能

    /// <summary>
    /// 初始化所有调整手柄的拖动事件
    /// </summary>
    private void InitializeResizeHandlers()
    {
        // 四边调整配置
        _resizeThumbs[0].DragDelta += (_, e) => AdjustWidth(-e.HorizontalChange, true);  // 左侧调整
        _resizeThumbs[1].DragDelta += (_, e) => AdjustWidth(e.HorizontalChange, false);  // 右侧调整
        _resizeThumbs[2].DragDelta += (_, e) => AdjustHeight(-e.VerticalChange, true);   // 顶部调整
        _resizeThumbs[3].DragDelta += (_, e) => AdjustHeight(e.VerticalChange, false);    // 底部调整

        // 四角调整配置（组合宽度和高度调整）
        _resizeThumbs[4].DragDelta += (_, e) => { AdjustWidth(-e.HorizontalChange, true); AdjustHeight(-e.VerticalChange, true); };  // 左上角
        _resizeThumbs[5].DragDelta += (_, e) => { AdjustWidth(e.HorizontalChange, false); AdjustHeight(-e.VerticalChange, true); };  // 右上角
        _resizeThumbs[6].DragDelta += (_, e) => { AdjustWidth(-e.HorizontalChange, true); AdjustHeight(e.VerticalChange, false); };  // 左下角
        _resizeThumbs[7].DragDelta += (_, e) => { AdjustWidth(e.HorizontalChange, false); AdjustHeight(e.VerticalChange, false); }; // 右下角
    }

    /// <summary>
    /// 调整控件宽度
    /// </summary>
    private void AdjustWidth(double delta, bool adjustLeft)
    {
        if (!(DataContext is BoxModel boxModel)) return;

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
            var newX = boxModel.X - widthDelta;
            boxModel.X = newX;
        }

        // 应用新宽度
        boxModel.Width = newWidth;
    }

    /// <summary>
    /// 调整控件高度
    /// </summary>
    private void AdjustHeight(double delta, bool adjustTop)
    {
        if (!(DataContext is BoxModel boxModel))
            return;

        // 计算原始新高度（当前高度加上变化量）
        var rawHeight = boxModel.Height + delta;

        // 计算边距总高度（顶部边距+标题栏高度）
        double marginTotal = MARGIN_SIZE + boxModel.HeadHeight;

        // 应用尺寸约束
        var newHeight = ApplySizeConstraint(rawHeight, SNAP_UNIT+16, marginTotal);

        // 确保不小于最小尺寸
        newHeight = Math.Max(newHeight, SNAP_UNIT + marginTotal + 16);

        // 计算实际高度变化量

        // 调整顶部位置
        if (adjustTop)
        {
            boxModel.Y -= (newHeight - boxModel.Height);
        }

        // 应用新高度
        boxModel.Height = newHeight;
        // Height = newHeight;
        // _expandedHeight = newHeight; // 保持缓存值更新
        // 使用字符串插值输出变量值
        // Debug.WriteLine($"newHeight: {newHeight}");
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

    #region 右键菜单功能
    
    ContextMenu _contextMenu = new();

    private void CreateContextMenu()
    {
        if (DataContext is not BoxModel boxModel) return;
        var mainViewModel = boxModel.Parent;
        
        var Aa = "";

        if (boxModel.IsExpanded == null)
        {
            Aa = "\uF16C";
        }
        else if (boxModel.IsExpanded == true)
        {
            Aa = "\uF16D";
        }
        else if (boxModel.IsExpanded == false)
        {
            Aa = "\uF16B";
        }
        
        _contextMenu.Items.Add(CreateMenuItem("新建盒子", "\uE710", mainViewModel.AddShelfCommand));
        _contextMenu.Items.Add(CreateMenuItem("移除盒子", "\uE74D", mainViewModel.RemoveShelfCommand, commandParameter: boxModel));
        // contextMenu.Items.Add(CreateMenuItem("图标显示"));
        // contextMenu.Items.Add(CreateMenuItem("列表显示"));
        // 一级菜单项（带子菜单）
        // contextMenu.Items.Add(CreateNestedMenuItem("盒子样式",
        //     CreateMenuItem("图标"),
        //     CreateMenuItem("列表"),
        //     CreateMenuItem("列表")
        // ));

        // _contextMenu.Items.Add(CreateMenuItem("占位符"));
        // contextMenu.Items.Add(CreateMenuItem("位置锁定"));
        _contextMenu.Items.Add(CreateMenuItem("自动盒子", Aa, boxModel.SetnullCommand));

        // 添加底部空白分隔项
        _contextMenu.Items.Add(new Separator());
        _contextMenu.Items.Add(CreateMenuItem("设置菜单", "\uE713", mainViewModel.OpenBoxSettingsCommand));
    }
    
    private void ContentArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 设置菜单显示位置
        _contextMenu.Placement = PlacementMode.MousePoint;

        // 显示菜单
        _contextMenu.IsOpen = true;

        // 标记事件已处理（防止冒泡）
        e.Handled = true;
    }


    // 独立菜单项创建函数
    private MenuItem CreateMenuItem(string header, string iconGlyph = null, ICommand? command = null, object commandParameter = null)
    {
        var menuItem = new MenuItem
        {
            Header = header,
            Command = command,
            CommandParameter = commandParameter,
            //Tag = "menu_custom" // 添加标识用于自动化测试
        };
        
        // 添加图标
        // if (iconResourceKey != null && Application.Current.Resources.Contains(iconResourceKey))
        // {
        //     menuItem.Icon = Application.Current.Resources[iconResourceKey];
        // }
        if (!string.IsNullOrEmpty(iconGlyph))
        {
            menuItem.Icon = new ContentControl
            {
                Template = (ControlTemplate)Resources["IconTemplate"],
                Tag = iconGlyph
            };
        }
        // 事件绑定
        //menuItem.Click += clickHandler;

        // 添加辅助功能支持
        // AutomationProperties.SetName(menuItem, $"{header}菜单项");

        return menuItem;
    }

    // 创建带子菜单的菜单项
    private MenuItem CreateNestedMenuItem(string header, params MenuItem[] children)
    {
        var menuItem = CreateMenuItem(header);
        menuItem.Items.Add(new Separator());
        foreach (var child in children)
        {
            menuItem.Items.Add(child);
        }
        return menuItem;
    }
    #endregion
}