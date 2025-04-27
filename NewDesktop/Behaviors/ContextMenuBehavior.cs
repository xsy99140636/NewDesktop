using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Microsoft.Xaml.Behaviors;
using NewDesktop.Shell;
using NewDesktop.ViewModels;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;

namespace NewDesktop.Behaviors;

public class ContextMenuBehavior : Behavior<ListView>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseRightButtonUp += OnRightClick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseRightButtonUp -= OnRightClick;
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
        // 获取点击位置相对于AssociatedObject的坐标
        var position = e.GetPosition(AssociatedObject);
    
        // 使用HitTest找到被点击的元素
        var hitResult = VisualTreeHelper.HitTest(AssociatedObject, position);
        if (hitResult == null) return;

        // 查找被点击的ListBoxItem（项容器）
        DependencyObject hitObject = hitResult.VisualHit;
        while (hitObject != null && !(hitObject is ListBoxItem))
        {
            hitObject = VisualTreeHelper.GetParent(hitObject);
        }
        
        if (hitObject == null) return; // 未点击在项上

        // 获取对应的数据项
        var itemContainer = (ListBoxItem)hitObject;
        var dataItem = AssociatedObject.ItemContainerGenerator.ItemFromContainer(itemContainer);
    
        // 检查数据项是否在选中列表中
        if (!AssociatedObject.SelectedItems.Contains(dataItem)) return;

        // 筛选有效路径（原有逻辑）
        var items = AssociatedObject.SelectedItems
            .Cast<IconModel>()
            .Where(icon => !string.IsNullOrEmpty(icon.Path) && (File.Exists(icon.Path) || Directory.Exists(icon.Path)))
            .Select(i => i.Path)
            .ToArray();
        if (items.Length == 0) return;

        // 获取鼠标位置
        var mousePosition = System.Windows.Forms.Control.MousePosition;
        var mousePoint = new Point(mousePosition.X, mousePosition.Y);
    
        // 获取窗口句柄并显示上下文菜单
        var mainWindow = Application.Current.MainWindow;
        var handle = new WindowInteropHelper(mainWindow).Handle;
        DesktopAttacher.ShowContextMenu(items, mousePoint, handle);
    
        e.Handled = true;
    }
}