using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NewDesktop.Views;

/// <summary>
/// 实现可拖动圆形控件，带边界限制和位置百分比输出功能
/// </summary>
[ObservableObject]
public partial class ColorPickerUserControl
{
    // 拖动状态控制
    private bool _isDragging; // 是否正在拖动的标志
    private Point _offset; // 鼠标点击点相对于图形左上角的偏移
        
    private bool _isUpdating = false;
        
    [ObservableProperty] private Color _hsvColor = Color.FromArgb(255, 255, 0, 0);
    [ObservableProperty] private Color _color = Color.FromArgb(255, 255, 255, 255);

    [ObservableProperty] private string _hexManual = "FFFFFFFF";

    [ObservableProperty] private byte _r = 255, _g = 255, _b = 255, _a = 255;

    [ObservableProperty] private double _h, _s = 1, _v = 1;

    [ObservableProperty] private double _x = -8, _y = -8;

    // 根据HSV值更新RGB和界面
    partial void OnHChanged(double value)
    {
        UpdateFromHsv();
        var (r1, g1, b1) = HsvToRgb(H, 1, 1);
        HsvColor = Color.FromArgb(255, r1, g1, b1);
    }
    partial void OnSChanged(double value) => UpdateFromHsv();
    partial void OnVChanged(double value) => UpdateFromHsv();

    // 当RGB值变化时更新HSV
    partial void OnRChanged(byte value) => UpdateFromRgb();
    partial void OnGChanged(byte value) => UpdateFromRgb();
    partial void OnBChanged(byte value) => UpdateFromRgb();
    partial void OnAChanged(byte value) => Color = Color.FromArgb(A, R, G, B);

    private void UpdateFromHsv()
    {
        if (_isUpdating) return;

        _isUpdating = true;
        try
        {
            var (r, g, b) = HsvToRgb(H, S, V);
            R = r;
            G = g;
            B = b; 
        }
        finally
        {
            _isUpdating = false;
        }

        Color = Color.FromArgb(A, R, G, B);
        HexManual = $"{A:X2}{R:X2}{G:X2}{B:X2}";
    }

    private void UpdateFromRgb()
    {
        if (_isUpdating) return;
        _isUpdating = true;
        Color = Color.FromArgb(A, R, G, B);
        try
        {
            var (h, s, v) = RgbToHsv(R, G, B);
            H = h;
            S = s;
            V = v;

            double canvasWidth = MyCanvas.ActualWidth;
            double canvasHeight = MyCanvas.ActualHeight;
            double circleWidth = ColorThumb.ActualWidth / 2;

            X = S * canvasWidth - circleWidth;
            Y = (1 - V) * canvasHeight - circleWidth;
        }
        finally
        {
            _isUpdating = false;
        }

        HexManual = $"{A:X2}{R:X2}{G:X2}{B:X2}";
    }

    public ColorPickerUserControl()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// 鼠标按下事件处理：开始拖动操作
    /// </summary>
    private void Circle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //var ellipse = sender as Ellipse;
        if (ColorThumb == null) return;

        // 进入拖动状态
        _isDragging = true;
        // 记录鼠标点击位置相对于图形左上角的偏移量
        _offset = e.GetPosition(ColorThumb);
        // 捕获鼠标以确保后续事件能正常接收
        ColorThumb.CaptureMouse();
    }

    /// <summary>
    /// 鼠标移动事件处理：实时更新圆形位置并限制边界
    /// </summary>
    private void Circle_MouseMove(object sender, MouseEventArgs e)
    {
        // 如果不在拖动退出
        if (!_isDragging) return;

        var ellipse = sender as Ellipse;

        // 获取鼠标在画布坐标系中的当前位置
        Point currentPos = e.GetPosition(MyCanvas);

        /* 位置计算 */
        // 计算理论上图形的新左上角坐标（未考虑边界）
        double newLeft = currentPos.X - _offset.X;
        double newTop = currentPos.Y - _offset.Y;

        /* 边界限制处理 */
        // 获取画布和图形的实际尺寸（使用ActualWidth确保获取渲染后的实际值）
        double canvasWidth = MyCanvas.ActualWidth;
        double canvasHeight = MyCanvas.ActualHeight;
        double circleWidth = ColorThumb.ActualWidth / 2;
        double circleHeight = ColorThumb.ActualHeight / 2;

        // 使用Clamp方法限制坐标范围：
        // X坐标范围：[负图形半径, 画布宽度 - 图形半径]] 防止右侧溢出
        // Y坐标范围：[负图形半径, 画布高度 - 图形半径] 防止底部溢出
        newLeft = Math.Clamp(newLeft, -circleWidth, canvasWidth - circleWidth);
        newTop = Math.Clamp(newTop, -circleWidth, canvasHeight - circleHeight);

        /* 更新图形位置 */
        X = newLeft;
        Y = newTop;

        S = (newLeft + circleWidth) / canvasWidth;
        V = 1 - (newTop + circleWidth) / canvasHeight;

        // 输出调试信息（实际发布时可替换为界面显示或其他日志方式）
        System.Diagnostics.Debug.WriteLine($"H: {H:F2}%, S:{S:F2}%, V:{V:F2}%");
    }

    /// <summary>
    /// 鼠标释放事件处理：结束拖动操作
    /// </summary>
    private void Circle_MouseUp(object sender, MouseButtonEventArgs e)
    {
        // 结束拖动状态
        _isDragging = false;
        //var ellipse = sender as Ellipse;
        // 释放鼠标捕获
        if (ColorThumb != null) ColorThumb.ReleaseMouseCapture();
    }

    #region 颜色转换

    /// <summary>
    /// 将HSV颜色转换为RGB颜色
    /// </summary>
    /// <param name="hue">色相 (0-360)</param>
    /// <param name="saturation">饱和度 (0-1)</param>
    /// <param name="value">明度 (0-1)</param>
    /// <returns>RGB值 (0-255)</returns>
    private static (byte R, byte G, byte B) HsvToRgb(double hue, double saturation, double value)
    {
        // 验证输入范围
        hue = Math.Clamp(hue, 0, 360);
        saturation = Math.Clamp(saturation, 0, 1);
        value = Math.Clamp(value, 0, 1);

        double c = value * saturation;
        double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
        double m = value - c;

        (double r, double g, double b) = hue switch
        {
            >= 0 and < 60 => (c, x, 0.0),
            >= 60 and < 120 => (x, c, 0),
            >= 120 and < 180 => (0, c, x),
            >= 180 and < 240 => (0, x, c),
            >= 240 and < 300 => (x, 0, c),
            >= 300 and <= 360 => (c, 0, x),
            _ => (0, 0, 0)
        };

        return (
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255)
        );
    }


    /// <summary>
    /// 将RGB颜色转换为HSV颜色
    /// </summary>
    /// <param name="r">红色分量 (0-255)</param>
    /// <param name="g">绿色分量 (0-255)</param>
    /// <param name="b">蓝色分量 (0-255)</param>
    /// <returns>包含H(0-360), S(0-1), V(0-1)的元组</returns>
    private static (double Hue, double Saturation, double Value) RgbToHsv(int r, int g, int b)
    {
        // 将RGB值归一化到0-1范围
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        double max = Math.Max(red, Math.Max(green, blue));
        double min = Math.Min(red, Math.Min(green, blue));
        double delta = max - min;

        double hue = 0;
        double saturation = 0;
        double value = max;

        // 计算色相(Hue)
        if (delta != 0)
        {
            if (max == red)
            {
                hue = (green - blue) / delta;
                if (green < blue) hue += 6;
            }
            else if (max == green)
            {
                hue = 2 + (blue - red) / delta;
            }
            else // max == blue
            {
                hue = 4 + (red - green) / delta;
            }

            hue *= 60;
        }

        // 计算饱和度(Saturation)
        if (max != 0)
        {
            saturation = delta / max;
        }

        return (hue, saturation, value);
    }

    #endregion
}