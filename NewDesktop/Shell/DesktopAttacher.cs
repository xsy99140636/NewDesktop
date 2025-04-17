using System.Windows;
using System.Windows.Interop;
using static NewDesktop.Shell.Interop.User32;

namespace NewDesktop.Shell;

public static partial class DesktopAttacher
{
    public const int GwlStyle = -16;
    
    public const int WsChild = 0x40000000;
    public const int WsVisible = 0x10000000;
    
    public const uint SwpShowwindow = 0x0040;
    public const uint SwpNozorder = 0x0004;
    
    public static void AttachToDesktop(Window targetWindow)
    {
        // 获取桌面组件句柄链
        IntPtr progman = FindWindow("Progman", "Program Manager");
        IntPtr defView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);

        // 如果找不到DefView，尝试查找WorkerW窗口
        if (defView == IntPtr.Zero)
        {
            IntPtr workerW = IntPtr.Zero;
            EnumWindows((hWnd, _) =>
            {
                if (FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero)
                {
                    workerW = hWnd;
                    return false; // 找到后停止枚举
                }
                return true;
            }, IntPtr.Zero);
            defView = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
        }

        // 获取当前窗口句柄
        IntPtr hWnd = new WindowInteropHelper(targetWindow).EnsureHandle();
        
        // 设置父窗口和样式
        if (defView != IntPtr.Zero)
        {
            SetParent(hWnd, defView);
            SetWindowLong(hWnd, GwlStyle, WsVisible);// WsChild | WsVisible);

            // 确保窗口覆盖整个屏幕
            // SetWindowPos(hWnd, IntPtr.Zero,
            //     0, 0,
            //     (int)SystemParameters.PrimaryScreenWidth,
            //     (int)SystemParameters.PrimaryScreenHeight,
            //     SWP_SHOWWINDOW | SWP_NOZORDER);
        }
    }
}