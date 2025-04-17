using System.Runtime.InteropServices;

namespace NewDesktop.Shell.Interop;

internal abstract class User32
{
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childAfter, string className, string? windowTitle);

    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    
    // 从文件系统路径创建PIDL(项目ID列表指针)
    [DllImport("shell32.dll")]
    public static extern IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath);

    // 释放PIDL
    [DllImport("shell32.dll")]
    public static extern void ILFree(IntPtr pidl);

    // 绑定到PIDL的父Shell文件夹
    [DllImport("shell32.dll")]
    public static extern int SHBindToParent(IntPtr pidl, ref Guid riid, out IntPtr ppv, out IntPtr ppidlLast);

    // 创建弹出菜单
    [DllImport("user32.dll")]
    public static extern IntPtr CreatePopupMenu();

    // 显示快捷菜单并跟踪选择
    [DllImport("user32.dll")]
    public static extern int TrackPopupMenuEx(IntPtr hMenu, uint uFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);

    // 销毁指定菜单
    [DllImport("user32.dll")]
    public static extern bool DestroyMenu(IntPtr hMenu);

    // 设置进程的DPI感知上下文
    [DllImport("user32.dll")]
    public static extern bool SetProcessDpiAwarenessContext(int value);
}
