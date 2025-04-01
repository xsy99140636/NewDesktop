using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace NewDesktop;

public partial class MainWindow
{
    #region Win32 API
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childAfter, string className, string windowTitle);

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    private const int GwlStyle = -16;
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const uint SwpShowwindow = 0x0040;
    private const uint SwpNozorder = 0x0004;
    #endregion

    public MainWindow()
    {
        InitializeComponent();
        // Loaded += (s, e) => AttachToDesktop();
    }

    private void AttachToDesktop()
    {
        // 获取桌面组件句柄链
        IntPtr progman = FindWindow("Progman", "Program Manager");
        IntPtr defView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);

        // 如果找不到DefView，尝试查找WorkerW窗口
        if (defView == IntPtr.Zero)
        {
            IntPtr workerW = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
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
        var helper = new WindowInteropHelper(this);
        IntPtr hWnd = helper.EnsureHandle();

        // 设置父窗口和样式
        if (defView != IntPtr.Zero)
        {
            SetParent(hWnd, defView);
            SetWindowLong(hWnd, GwlStyle, WsChild | WsVisible);
            
            // 确保窗口覆盖整个屏幕
            // SetWindowPos(hWnd, IntPtr.Zero,
            //     0, 0,
            //     (int)SystemParameters.PrimaryScreenWidth,
            //     (int)SystemParameters.PrimaryScreenHeight,
            //     SWP_SHOWWINDOW | SWP_NOZORDER);
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    // private void Window_DragOver(object sender, DragEventArgs e)
    // {
    //     // 检查是否是文件拖放
    //     if (e.Data.GetDataPresent(DataFormats.FileDrop))
    //     {
    //         e.Effects = DragDropEffects.Copy; // 显示复制图标
    //     }
    //     else
    //     {
    //         e.Effects = DragDropEffects.None;
    //     }
    //     e.Handled = true;
    // }

    // private void Window_Drop(object sender, DragEventArgs e)
    // {
    //     if (e.Data.GetDataPresent(DataFormats.FileDrop))
    //     {
    //         string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
    //         foreach (string file in files)
    //         {
    //             // 创建新的 Icon 并添加到界面
    //             var icon = new Icon
    //             {
    //                 Name = System.IO.Path.GetFileNameWithoutExtension(file),
    //                 X = e.GetPosition(this).X, // 放在鼠标位置
    //                 Y = e.GetPosition(this).Y
    //             };
    //
    //             if (DataContext is MainViewModel vm)
    //             {
    //                 vm.Entities.Add(new IconModel(icon));
    //             }
    //         }
    //     }
    // }
}