using System.Runtime.InteropServices;

namespace NewDesktop.Shell.Interop;

internal static class ComInterfaces
{
    /// <summary>
    /// 屏幕坐标的POINT结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    /// <summary>
    /// 扩展命令调用信息结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CMINVOKECOMMANDINFOEX
    {
        public int cbSize; // 结构体大小
        public int fMask; // 控制调用的标志
        public IntPtr hwnd; // 所有者窗口句柄
        public IntPtr lpVerb; // 要调用的命令(ANSI)
        public IntPtr lpParameters; // 参数(ANSI)
        public IntPtr lpDirectory; // 工作目录(ANSI)
        public int nShow; // 显示窗口命令
        public int dwHotKey; // 热键
        public IntPtr hIcon; // 图标句柄
        public IntPtr lpTitle; // 标题(ANSI)
        public IntPtr lpVerbW; // 要调用的命令(Unicode)
        public IntPtr lpParametersW; // 参数(Unicode)
        public IntPtr lpDirectoryW; // 工作目录(Unicode)
        public IntPtr lpTitleW; // 标题(Unicode)
        public POINT ptInvoke; // 调用命令的点
    }
    
    /// <summary>
    /// IShellFolder接口 - 用于与Shell文件夹交互
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    public interface IShellFolder
    {
        // [PreserveSig]
        // int ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, out uint pchEaten, out IntPtr ppidl, out uint pdwAttributes);

        // [PreserveSig]
        // int EnumObjects(IntPtr hwnd, uint grfFlags, out IntPtr ppenumIDList);

        // [PreserveSig]
        // int BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        // [PreserveSig]
        // int BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        // [PreserveSig]
        // int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

        // [PreserveSig]
        // int CreateViewObject(IntPtr hwndOwner, ref Guid riid, out IntPtr ppv);

        // [PreserveSig]
        // int GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] apidl, out uint rgfInOut);

        [PreserveSig]
        int GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] apidl, ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);

        // [PreserveSig]
        // int GetDisplayNameOf(IntPtr pidl, uint uFlags, IntPtr pName);

        // [PreserveSig]
        // int SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
    }

    /// <summary>
    /// IContextMenu3接口 - 上下文菜单接口的扩展版本
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("BCFCE0A0-EC17-11D0-8D10-00A0C90F2719")]
    public interface IContextMenu3
    {
        [PreserveSig]
        int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);

        [PreserveSig]
        int InvokeCommand(ref CMINVOKECOMMANDINFOEX pici);

        // [PreserveSig]
        // int GetCommandString(uint idCmd, uint uType, uint pReserved, IntPtr pszName, uint cchMax);
    }
}