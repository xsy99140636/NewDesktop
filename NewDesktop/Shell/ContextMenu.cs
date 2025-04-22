using System.Runtime.InteropServices;
using System.Windows;
using static NewDesktop.Shell.Interop.User32;

namespace NewDesktop.Shell
{
    /// <summary>
    /// 用于在WPF应用程序中显示Windows Shell文件/文件夹右键菜单的类
    /// </summary>
    public partial class DesktopAttacher
    {
        // TrackPopupMenuEx 函数的标志常量
        private const uint TPM_RETURNCMD = 0x0100;      // 返回选择的命令标识符
        private const uint TPM_RIGHTBUTTON = 0x0002;    // 右键菜单行为
        private const uint TPM_NONOTIFY = 0x0080;       // 不发送通知消息
        private const uint TPM_VERNEGANIMATION = 0x2000; // 禁用菜单动画

        // QueryContextMenu 函数的标志常量
        private const uint CMF_NORMAL = 0x00000000;          // 正常菜单显示
        private const uint CMF_DEFAULTONLY = 0x00000001;     // 只显示默认命令
        private const uint CMF_VERBSONLY = 0x00000002;       // 只显示动词命令
        private const uint CMF_EXPLORE = 0x00000004;         // 资源管理器样式菜单
        private const uint CMF_NOVERBS = 0x00000008;         // 不显示动词命令
        private const uint CMF_CANRENAME = 0x00000010;       // 允许重命名命令
        private const uint CMF_NODEFAULT = 0x00000020;       // 无默认项
        private const uint CMF_INCLUDESTATIC = 0x00000040;   // 包含静态项
        private const uint CMF_ITEMMENU = 0x00000080;        // 项目上下文菜单
        private const uint CMF_EXTENDEDVERBS = 0x00000100;   // 扩展动词
        private const uint CMF_DISABLEDVERBS = 0x00000200;   // 禁用动词
        private const uint CMF_ASYNCVERBSTATE = 0x00000400;  // 异步动词状态
        private const uint CMF_OPTIMIZEFORINVOKE = 0x00000800; // 优化调用
        private const uint CMF_SYNCCASCADEMENU = 0x00001000; // 同步级联菜单
        private const uint CMF_DONOTPICKDEFAULT = 0x00002000; // 不选择默认项
        private const uint CMF_RESERVED = 0xffff0000;        // 保留标志

        /// <summary>
        /// 在指定位置显示指定文件的Windows Shell右键菜单
        /// </summary>
        /// <param name="filePaths">要显示右键菜单的文件路径数组</param>
        /// <param name="location">菜单应该出现的屏幕坐标</param>
        /// <param name="handle">接收菜单消息的窗口句柄</param>
        public static void ShowContextMenu(string[] filePaths, Point location, IntPtr handle)
        {
            if (filePaths == null || filePaths.Length == 0) return;

            // 设置高DPI感知以确保正确的菜单位置
            SetProcessDpiAwarenessContext(-4); // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2

            IntPtr shellFolder = IntPtr.Zero;    // IShellFolder接口指针
            IntPtr contextMenuPtr = IntPtr.Zero; // IContextMenu接口指针
            IntPtr hmenu = IntPtr.Zero;         // 菜单句柄

            try
            {
                // 存放PIDL(项目ID列表指针)的数组
                IntPtr[] pidls = new IntPtr[filePaths.Length];
                IntPtr[] pidlLasts = new IntPtr[filePaths.Length];

                // 为每个文件路径创建PIDL
                for (int i = 0; i < filePaths.Length; i++)
                {
                    pidls[i] = ILCreateFromPath(filePaths[i]);
                    if (pidls[i] == IntPtr.Zero) return; // 创建PIDL失败
                }

                // IShellFolder接口的GUID
                Guid guidShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");

                // 绑定到第一个项目的父文件夹获取IShellFolder接口
                if (SHBindToParent(pidls[0], ref guidShellFolder, out shellFolder, out pidlLasts[0]) != 0)
                    return; // 绑定失败

                // 从指针获取IShellFolder接口
                IShellFolder folder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(shellFolder, typeof(IShellFolder));

                // 获取所有项目的PIDL lasts
                for (int i = 1; i < filePaths.Length; i++)
                {
                    IntPtr tempShellFolder;
                    if (SHBindToParent(pidls[i], ref guidShellFolder, out tempShellFolder, out pidlLasts[i]) == 0)
                    {
                        Marshal.Release(tempShellFolder); // 释放临时Shell文件夹
                    }
                }

                // IContextMenu接口的GUID
                Guid guidContextMenu = new Guid("000214e4-0000-0000-c000-000000000046");

                // 获取选定项目的上下文菜单接口
                folder.GetUIObjectOf(handle, (uint)filePaths.Length, pidlLasts, ref guidContextMenu, IntPtr.Zero, out contextMenuPtr);

                if (contextMenuPtr != IntPtr.Zero)
                {
                    // 获取IContextMenu3接口
                    IContextMenu3 contextMenu = (IContextMenu3)Marshal.GetTypedObjectForIUnknown(contextMenuPtr, typeof(IContextMenu3));

                    // 创建弹出菜单
                    hmenu = CreatePopupMenu();

                    if (hmenu != IntPtr.Zero)
                    {
                        // 用上下文项填充菜单
                        contextMenu.QueryContextMenu(
                            hmenu,
                            0,          // 开始添加菜单项的索引
                            1,          // 最小命令ID
                            0x7FFF,     // 最大命令ID
                            CMF_ITEMMENU | CMF_NORMAL | CMF_ASYNCVERBSTATE | CMF_CANRENAME
                            );

                        // 显示菜单并跟踪选择
                        uint selected = (uint)TrackPopupMenuEx(hmenu,
                            TPM_RETURNCMD | TPM_RIGHTBUTTON,
                            (int)location.X,    // X坐标
                            (int)location.Y,    // Y坐标
                            handle,             // 所有者窗口
                            IntPtr.Zero);       // 无扩展参数

                        // 如果用户选择了项目
                        if (selected > 0)
                        {
                            // 准备命令调用结构
                            var invokeInfo = new CMINVOKECOMMANDINFOEX
                            {
                                cbSize = Marshal.SizeOf(typeof(CMINVOKECOMMANDINFOEX)),
                                fMask = 0x00004000 | 0x20000000,  // CMIC_MASK_UNICODE | CMIC_MASK_PTINVOKE
                                hwnd = handle,                    // 所有者窗口
                                lpVerb = (IntPtr)(selected - 1),  // 要调用的命令(基于0)
                                lpVerbW = (IntPtr)(selected - 1), // Unicode命令
                                nShow = 1,                       // SW_SHOWNORMAL
                                ptInvoke = new POINT { x = (int)location.X, y = (int)location.Y } // 调用点
                            };

                            // 执行选定的命令
                            contextMenu.InvokeCommand(ref invokeInfo);
                        }
                    }
                }
            }
            finally
            {
                // 清理资源
                if (hmenu != IntPtr.Zero) DestroyMenu(hmenu);
                if (contextMenuPtr != IntPtr.Zero) Marshal.Release(contextMenuPtr);
                if (shellFolder != IntPtr.Zero) Marshal.Release(shellFolder);

                // 注意: PIDL由调用者根据需要释放
            }
        }

        /// <summary>
        /// IShellFolder接口 - 用于与Shell文件夹交互
        /// </summary>
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214E6-0000-0000-C000-000000000046")]
        private interface IShellFolder
        {
            [PreserveSig]
            int ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
                out uint pchEaten, out IntPtr ppidl, out uint pdwAttributes);

            [PreserveSig]
            int EnumObjects(IntPtr hwnd, uint grfFlags, out IntPtr ppenumIDList);

            [PreserveSig]
            int BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

            [PreserveSig]
            int BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

            [PreserveSig]
            int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

            [PreserveSig]
            int CreateViewObject(IntPtr hwndOwner, ref Guid riid, out IntPtr ppv);

            [PreserveSig]
            int GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] apidl, out uint rgfInOut);

            [PreserveSig]
            int GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] apidl,
                ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);

            [PreserveSig]
            int GetDisplayNameOf(IntPtr pidl, uint uFlags, IntPtr pName);

            [PreserveSig]
            int SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
        }

        /// <summary>
        /// IContextMenu3接口 - 上下文菜单接口的扩展版本
        /// </summary>
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("BCFCE0A0-EC17-11D0-8D10-00A0C90F2719")]
        private interface IContextMenu3
        {
            [PreserveSig]
            int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);

            [PreserveSig]
            int InvokeCommand(ref CMINVOKECOMMANDINFOEX pici);

            [PreserveSig]
            int GetCommandString(uint idCmd, uint uType, uint pReserved, IntPtr pszName, uint cchMax);
        }

        /// <summary>
        /// 屏幕坐标的POINT结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 扩展命令调用信息结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CMINVOKECOMMANDINFOEX
        {
            public int cbSize;             // 结构体大小
            public int fMask;               // 控制调用的标志
            public IntPtr hwnd;             // 所有者窗口句柄
            public IntPtr lpVerb;           // 要调用的命令(ANSI)
            public IntPtr lpParameters;     // 参数(ANSI)
            public IntPtr lpDirectory;      // 工作目录(ANSI)
            public int nShow;               // 显示窗口命令
            public int dwHotKey;            // 热键
            public IntPtr hIcon;            // 图标句柄
            public IntPtr lpTitle;          // 标题(ANSI)
            public IntPtr lpVerbW;          // 要调用的命令(Unicode)
            public IntPtr lpParametersW;     // 参数(Unicode)
            public IntPtr lpDirectoryW;     // 工作目录(Unicode)
            public IntPtr lpTitleW;         // 标题(Unicode)
            public POINT ptInvoke;          // 调用命令的点
        }
    }
}