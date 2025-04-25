using NewDesktop.ViewModels;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NewDesktop.Behaviors;

/// <summary>
/// 提供ListView图标的拖放功能行为
/// 通过附加属性控制拖放功能的启用/禁用，并绑定拖放操作的处理命令
/// </summary>
public static class IconDragDropBehavior
{
    private static IconModel[] selectedItems;

    #region IsEnabled 附加属性
    /// <summary>
    /// 控制是否启用拖放行为的附加属性
    /// 注册为附加属性，可应用于任何DependencyObject（通常用于ListView）
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(IconDragDropBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged) // 默认值false，属性变化回调
        );

    /// <summary>
    /// 获取IsEnabled附加属性的值
    /// </summary>
    /// <param name="obj">目标依赖对象（通常为ListView）</param>
    /// <returns>当前是否启用拖放功能</returns>
    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    /// <summary>
    /// 设置IsEnabled附加属性的值
    /// </summary>
    /// <param name="obj">目标依赖对象</param>
    /// <param name="value">是否启用拖放功能</param>
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);
    #endregion

    #region DropHandler 附加属性
    /// <summary>
    /// 绑定拖放操作处理命令的附加属性
    /// 当拖放操作完成时，会执行此命令
    /// </summary>
    public static readonly DependencyProperty DropHandlerProperty =
        DependencyProperty.RegisterAttached(
            "DropHandler",
            typeof(ICommand),
            typeof(IconDragDropBehavior) // 无默认值，无属性变化回调
        );


    /// <summary>
    /// 获取拖放处理命令
    /// </summary>
    /// <param name="obj">目标依赖对象</param>
    /// <returns>当前绑定的处理命令</returns>
    public static ICommand GetDropHandler(DependencyObject obj) => (ICommand)obj.GetValue(DropHandlerProperty);

    /// <summary>
    /// 设置拖放处理命令
    /// </summary>
    /// <param name="obj">目标依赖对象</param>
    /// <param name="value">要绑定的处理命令</param>
    public static void SetDropHandler(DependencyObject obj, ICommand value) => obj.SetValue(DropHandlerProperty, value);
    #endregion

    #region 事件处理
    /// <summary>
    /// IsEnabled属性变化回调方法
    /// </summary>
    /// <param name="d">目标对象（预期为ListView）</param>
    /// <param name="e">属性变化事件参数</param>
    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {

        // 仅处理ListView控件
        if (d is not ListView listView) return;

        // 根据新值启用/禁用拖放功能
        if ((bool)e.NewValue)
        {
            // 注册鼠标移动事件（用于启动拖拽操作）
            listView.PreviewMouseLeftButtonDown += OnPreviewMouseDown;
            // listView.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonDown;
            listView.PreviewMouseMove += OnPreviewMouseMove;
            listView.DragEnter += ListViewOnDragEnter;
            listView.Drop += OnDrop;            // 注册拖放完成事件
            // 启用控件的拖放支持
            listView.AllowDrop = true;
        }
        
        else
        {
            // 移除事件监听
            listView.PreviewMouseLeftButtonDown -= OnPreviewMouseDown;
            listView.PreviewMouseMove -= OnPreviewMouseMove;
            listView.Drop -= OnDrop;
            listView.AllowDrop = false;
        }
    }
    
    private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListView listView) return;

        selectedItems = listView.SelectedItems            // 获取所有选中的 IconModel
            .OfType<IconModel>()
            .Where(item => !string.IsNullOrEmpty(item.Path))
            .ToArray();
        // e.Handled = true;
    }

    /// <summary>
    /// 鼠标移动事件处理（待实现）
    /// 计划在此处处理拖拽操作的初始化
    /// </summary>
    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        // TODO: 实现拖拽启动逻辑
        // 可能需要检测鼠标左键状态，设置DragDrop效果
        // if (e.LeftButton == MouseButtonState.Pressed)
        // {
        //     var listView = sender as ListView;
        //     if (listView == null) return;
        //     var item = listView.SelectedItem;
        //     if (item == null) return;
        //     
        //     var iconViewModel = item as IconModel;
        //     if (iconViewModel == null) return;
        //     
        //     // 获取鼠标位置
        //     Point mousePos = e.GetPosition(null);
        //     // 获取拖放数据
        //     var dragData = new DataObject(DataFormats.FileDrop, new string[] { iconViewModel.Path });
        //     // 启动拖放操作
        //     DragDrop.DoDragDrop(sender as ListView, dragData, DragDropEffects.Copy);
        // }
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            
            var listView = sender as ListView;
            if (listView == null) return;
            
            if (selectedItems.Length == 0) return;

            // 收集所有文件路径
            var filePaths = selectedItems.Select(item => item.Path).ToArray();

            // 创建拖放数据对象
            var dragData = new DataObject(DataFormats.FileDrop, filePaths);
            //dragData.SetData("Preferred DropEffect", DragDropEffects.Copy);

            // 启动拖放操作
            var result = DragDrop.DoDragDrop(listView, dragData, DragDropEffects.Move| DragDropEffects.Copy| DragDropEffects.Link);

            if (result == DragDropEffects.Copy)
            {
                Debug.WriteLine("Copy");
            }
            if (result == DragDropEffects.Move)
            {
                Debug.WriteLine("Move");
                // 如果执行移动，需从数据源中删除原数据
                if (listView.ItemsSource is IList<IconModel> sourceCollection)
                {
                    foreach (var item in selectedItems)
                    {
                        sourceCollection.Remove(item);
                    }
                }
                Debug.WriteLine($"{listView.Items}");
                Debug.WriteLine($"{listView.ItemsSource}");
            }
            if (result == DragDropEffects.Link)
            {
                Debug.WriteLine("Link");
            }
            if (result == DragDropEffects.None)
            {
                Debug.WriteLine("None");
            }
            if (result == DragDropEffects.Scroll)
            {
                Debug.WriteLine("Scroll");
            }

        }
    }

    private static void ListViewOnDragEnter(object sender, DragEventArgs e)
    {
        // // throw new NotImplementedException();
        // Debug.WriteLine("测试");
        //
        // if (e.Data.GetDataPresent(DataFormats.FileDrop))
        // {
        //     e.Effects = DragDropEffects.Copy; // 允许复制操作
        // }
        // else
        // {
        //     e.Effects = DragDropEffects.None; // 不支持其他类型
        // }
        // e.Handled = true; // 标记事件已处理
        // e.Effects = DragDropEffects.Move;
        // Debug.WriteLine($"{e.Effects}");
        
    }
    
    /// <summary>
    /// 拖放完成事件处理（待实现）
    /// 计划在此处处理拖放数据的提取和命令执行
    /// </summary>
    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not ListView targetListView) return;

        // 确保拖放的数据包含文件
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
    
        // 获取文件路径数组
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        Debug.WriteLine($"{e.Effects}");
        // if (e.Effects == DragDropEffects.Move)
        // {
            // 获取绑定的命令并执行，传递文件路径作为参数
            ICommand dropCommand = GetDropHandler(targetListView);


        // 构建包含坐标的命令参数
        var commandData = new DropCommandData
        {
            sender = sender,
            e = e
        };


        if (dropCommand != null && dropCommand.CanExecute(commandData))
            {
                dropCommand.Execute(commandData);
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // 检查是否按下Ctrl键进行复制操作
                    bool isCtrlPressed = (e.KeyStates & DragDropKeyStates.ControlKey) != 0;
                    e.Effects = isCtrlPressed ? DragDropEffects.Copy : DragDropEffects.Move;
                }
            }

    }
    #endregion
    public class DropCommandData
    {
        public object sender { get; set; }
        public DragEventArgs e { get; set; }
    }
}
