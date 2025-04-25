using System.Diagnostics;
using System.IO;
using System.Windows;
using NewDesktop.ViewModels;

namespace NewDesktop.Views;

public partial class IconView
{
    public IconView()
    {
        InitializeComponent();
    }

    private void IconView_OnDrop(object sender, DragEventArgs e)
    {
        if (!(DataContext is IconModel iconData)) return;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (Path.GetExtension(iconData.Path) == ".exe" || Path.GetExtension(iconData.Path) == ".lnk")
            {
                // string[] file = { "A:\\Desktop\\qwqqwww.svg", "A:\\Desktop\\dwadwwda.svg" };

                // 为每个路径添加双引号并用空格分隔
                string arguments = string.Join(" ", files.Select(f => $"\"{f}\""));
                var startInfo = new ProcessStartInfo
                {
                    FileName = @iconData.Path,
                    Arguments = arguments,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            else if (Directory.Exists(iconData.Path))
            {
                Debug.WriteLine("wj");
            }
            // Handle the dropped file
            // MessageBox.Show(file);
        }

        e.Handled = true;
    }

    private void UserControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        //if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        //{
        //    var dragData = new DataObject(DataFormats.FileDrop, new string[] { "A:\\7-Zip\\History.txt" });
        //    DragDrop.DoDragDrop(this, dragData, DragDropEffects.Copy);
        //    // Handle mouse move event
        //    // For example, initiate drag-and-drop operation
        //    // MessageBox.Show("Mouse moved with left button pressed");
        //}
    }
}