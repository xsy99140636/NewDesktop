using System.Runtime.InteropServices;
using NewDesktop.Shell;

namespace NewDesktop;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        // Loaded += (s, e) => AttachToDesktop();
        Loaded += (s, e) => DesktopAttacher.AttachToDesktop(this);
    }

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