using NewDesktop.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace NewDesktop.Behaviors;

/// <summary>
/// 提供ListView图标的拖放功能行为
/// 通过附加属性控制拖放功能的启用/禁用，并绑定拖放操作的处理命令
/// </summary>
public static class IconDragDropBehavior
{
    // private static IconModel[]? _selectedItems;

    private static bool _isBlankSpace;
    private static bool _isDragging;
    
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
    
    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    
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
    
    public static ICommand GetDropHandler(DependencyObject obj) => (ICommand)obj.GetValue(DropHandlerProperty);

    public static void SetDropHandler(DependencyObject obj, ICommand value) => obj.SetValue(DropHandlerProperty, value);

    #endregion

    #region DropHandler 附加属性

    /// <summary>
    /// 绑定拖放操作处理命令的附加属性
    /// 当拖放操作完成时，会执行此命令
    /// </summary>
    public static readonly DependencyProperty NewBoxProperty =
        DependencyProperty.RegisterAttached(
            "NewBox",
            typeof(ICommand),
            typeof(IconDragDropBehavior) // 无默认值，无属性变化回调
        );
    
    public static ICommand GetNewBox(DependencyObject obj) => (ICommand)obj.GetValue(NewBoxProperty);

    public static void SetNewBox(DependencyObject obj, ICommand value) => obj.SetValue(NewBoxProperty, value);

    #endregion
    
// region 内部使用的附加属性（仅限本行为类内部访问）

    /// <summary>
    /// 用于存储当前 ListView 关联的拖拽选择装饰器实例
    /// </summary>
    private static readonly DependencyProperty AdornerProperty =
        DependencyProperty.RegisterAttached(
            "Adorner",
            typeof(DragSelectAdorner),
            typeof(IconDragDropBehavior));

    /// <summary>
    /// 用于存储选择矩形（Rectangle）的引用
    /// </summary>
    private static readonly DependencyProperty SelectionRectProperty =
        DependencyProperty.RegisterAttached(
            "SelectionRect",
            typeof(Rectangle),
            typeof(IconDragDropBehavior));

    /// <summary>
    /// 记录拖拽操作的起始坐标点
    /// </summary>
    private static readonly DependencyProperty DragStartProperty =
        DependencyProperty.RegisterAttached(
            "DragStart",
            typeof(Point?),
            typeof(IconDragDropBehavior));

// endregion
    
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

        // 移除旧的订阅防止重复
        listView.Loaded -= OnListViewLoaded;
        listView.Unloaded -= OnListViewUnloaded;

        // 根据新值启用/禁用拖放功能
        if ((bool)e.NewValue)
        {
            // 订阅加载/卸载事件
            listView.Loaded += OnListViewLoaded;
            listView.Unloaded += OnListViewUnloaded;
            // 如果已经加载，立即初始化
            if (listView.IsLoaded)
            {
                InitializeBehavior(listView);
            }

            // 注册鼠标移动事件（用于启动拖拽操作）
            listView.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            listView.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            listView.PreviewMouseMove += OnPreviewMouseMove;
            listView.DragEnter += ListViewOnDragEnter;
            listView.MouseLeave += ListViewOnMouseLeave;
            listView.Drop += OnDrop;
            listView.AllowDrop = true;
        }

        else
        {
            CleanupBehavior(listView);

            // 移除事件监听
            listView.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            listView.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            listView.PreviewMouseMove -= OnPreviewMouseMove;
            listView.DragEnter -= ListViewOnDragEnter;
            listView.Drop -= OnDrop;
            listView.AllowDrop = true;
        }
    }

    


    #region 核心方法

    // 初始化拖拽选择行为
    private static void InitializeBehavior(ListView listView)
    {
        // 创建半透明选择矩形
        var selectionRect = new Rectangle
        {
            Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),// Brushes.DodgerBlue, // 边框颜色
            StrokeThickness = 0.5, // 边框粗细
            Fill = new SolidColorBrush(Color.FromArgb(70, 0, 102, 204)), // 半透明填充
            Visibility = Visibility.Collapsed, // 初始隐藏
            IsHitTestVisible = false // 不阻挡鼠标事件
        };

        // 获取装饰层并添加自定义装饰器
        if (AdornerLayer.GetAdornerLayer(listView) is not {} adornerLayer) return;
        
        var adorner = new DragSelectAdorner(listView, selectionRect);
        adornerLayer.Add(adorner);

        // 存储引用供后续使用
        listView.SetValue(AdornerProperty, adorner);
        listView.SetValue(SelectionRectProperty, selectionRect);

    }

    // 清理行为相关资源
    private static void CleanupBehavior(ListView listView)
    {
        // 移除装饰器
        if (listView.GetValue(AdornerProperty) is DragSelectAdorner adorner)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(listView);
            adornerLayer?.Remove(adorner);
            listView.ClearValue(AdornerProperty);
        }

        // 清除附加属性

        listView.ClearValue(DragStartProperty);
        listView.ClearValue(SelectionRectProperty);
    }

    #endregion


    // ListView加载完成时初始化行为
    private static void OnListViewLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ListView listView) InitializeBehavior(listView);
    }

    // ListView卸载时清理资源
    private static void OnListViewUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is ListView listView) CleanupBehavior(listView);
    }


    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // var listView = sender as ListView;
        // if (listView == null) return;
        if (sender is ListView listView && listView.GetValue(SelectionRectProperty) is Rectangle selectionRect)
        {
            // 记录鼠标按下位置
            var mouseDownPos = e.GetPosition(null);

            // 记录拖拽起始点（相对于装饰层）
            listView.SetValue(DragStartProperty, mouseDownPos);
            
            // 命中结果
            if (VisualTreeHelper.HitTest(listView, e.GetPosition(listView)) is not { } hitResult) return;
            var hitObject = hitResult.VisualHit;// 命中对象
            
            //     hitObject不为空    且  hitObject是不属于ListViewItem类型 就继续循环 查找命中对象父项
            while (hitObject != null && hitObject is not ListViewItem) hitObject = VisualTreeHelper.GetParent(hitObject);

            if (hitObject is ListViewItem listViewItem)// 点击的是列表项
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != 0) // 多选添加
                {
                }
                else // 单多选添加
                {
                    if (listView.SelectedItems.Count < 2) listView.SelectedItems.Clear();

                    listViewItem.IsSelected = true;
                }
                _isDragging = true;// 正在拖动
            }
            else// 点击的不是列表项
            {
                if (AdornerLayer.GetAdornerLayer(listView) is not { } adornerLayer) return;

                var dragStart = e.GetPosition(adornerLayer);
                listView.SetValue(DragStartProperty, dragStart);
                
                // 初始化选择矩形位置
                Canvas.SetLeft(selectionRect, dragStart.X);
                Canvas.SetTop(selectionRect, dragStart.Y);
                selectionRect.Width = 0;
                selectionRect.Height = 0;
                selectionRect.Visibility = Visibility.Visible;

                // 如果没有按住Ctrl/Shift键，清空原有选择
                if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == 0) listView.SelectedItems.Clear();
                
                _isBlankSpace = true;// 是空白区域
            }

            e.Handled = true;
            
        }
    }

    private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // _isBlankSpace = false;
        _isDragging = false;
        
        if (sender is not ListView listView) return;

        if (_isBlankSpace)
        {
            _isBlankSpace = false;
            
            if (listView.GetValue(SelectionRectProperty) is Rectangle selectionRect)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != 0)
                {
                    var newBoxCoordinates = new NewBoxData
                    {
                        Size = new Point(selectionRect.Height, selectionRect.Width),
                        Position = new Point(Canvas.GetLeft(selectionRect), Canvas.GetTop(selectionRect)),
                    };

                    ICommand newBoxCommand = GetNewBox(listView);


                    // todo
                    if (newBoxCommand.CanExecute(newBoxCoordinates))
                    {
                        newBoxCommand.Execute(newBoxCoordinates);
                        
                    }
                }
                
                selectionRect.Visibility = Visibility.Collapsed;
                listView.ClearValue(DragStartProperty);
                listView.ReleaseMouseCapture();
                // e.Handled = true;
                
            }
            return;
            // var newBoxCoordinates = new Point(1, 1); // 创建坐标点
        }
        
        // 进行视觉树命中测试，获取当前鼠标位置的元素信息
        var hitResult = VisualTreeHelper.HitTest(listView, e.GetPosition(listView));
        if (hitResult == null) return;

        // 在视觉树中向上查找最近的ListViewItem容器
        var hitObject = hitResult.VisualHit;
        while (hitObject != null && hitObject is not ListViewItem) hitObject = VisualTreeHelper.GetParent(hitObject);

        if (hitObject is ListViewItem listViewItem)
        {
            // 检测当前是否按下多选组合键（Ctrl或Shift）
            bool isMultiSelectKey = (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != 0;
            
            if (isMultiSelectKey) listViewItem.IsSelected = !listViewItem.IsSelected;
            else
            {
                if (listView.SelectedItems.Count > 2)
                {
                    listView.SelectedItems.Clear();
                    listViewItem.IsSelected = true;
                }
            }
        }
    }

    private static void ListViewOnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is ListView listView &&
            listView.GetValue(SelectionRectProperty) is Rectangle selectionRect)
        {
            selectionRect.Visibility = Visibility.Collapsed;
            listView.ClearValue(DragStartProperty);
            listView.ReleaseMouseCapture();
            // listView.SelectedItems.Clear();
            // e.Handled = true;
        }
    }
    
    /// <summary>
    /// 鼠标移动事件处理（待实现）
    /// 计划在此处处理拖拽操作的初始化
    /// </summary>
    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && sender is ListView listView && listView.GetValue(DragStartProperty) is Point dragStart)
        {
            if (_isBlankSpace && listView.GetValue(SelectionRectProperty) is Rectangle selectionRect )// 是空白区域且从ListView的依赖属性中获取的选择对象类型是矩形。
            {
                if (AdornerLayer.GetAdornerLayer(listView) is not {} adornerLayer) return;
                
                // 获取当前鼠标位置（相对装饰层）
                var pos = e.GetPosition(adornerLayer);

                // 计算矩形区域（确保正数宽高）
                var x = Math.Min(pos.X, dragStart.X);
                var y = Math.Min(pos.Y, dragStart.Y);
                var width = Math.Abs(pos.X - dragStart.X);
                var height = Math.Abs(pos.Y - dragStart.Y);

                // 更新选择矩形位置和尺寸
                Canvas.SetLeft(selectionRect, x);
                Canvas.SetTop(selectionRect, y);
                selectionRect.Width = width;
                selectionRect.Height = height;

                // 查找滚动视图并计算实际选择区域
                if (FindVisualChild<ScrollViewer>(listView) is not { } scrollViewer) return;

                // 调整选择区域坐标（考虑滚动偏移）
                var selectionArea = new Rect(
                    x - scrollViewer.HorizontalOffset,
                    y - scrollViewer.VerticalOffset,
                    width,
                    height);

                // 根据区域选择列表项
                SelectItemsInArea(listView, selectionArea);
                // e.Handled = true;
            }
            // if (e.LeftButton == MouseButtonState.Pressed )
            else if (_isDragging)
            {

                var selectedItems = listView.SelectedItems // 获取所有选中的 IconModel
                    .OfType<IconModel>()
                    .Where(item => !string.IsNullOrEmpty(item.Path))
                    .ToArray();

                // _isDragging = true;
                // e.Handled = true;

                if (selectedItems.Length == 0) return;

                // 收集所有文件路径
                var filePaths = selectedItems.Select(item => item.Path).ToArray();

                // 创建拖放数据对象
                var dragData = new DataObject(DataFormats.FileDrop, filePaths);
                //dragData.SetData("Preferred DropEffect", DragDropEffects.Copy);
                
                // 启动拖放操作
                var result = DragDrop.DoDragDrop(listView, dragData, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Link);

                if (result == DragDropEffects.Copy)
                {
                    Debug.WriteLine("Copy");
                }

                if (result == DragDropEffects.Move)
                {
                    Debug.WriteLine("Move");
                    // 如果执行移动，需从数据源中删除原数据
                    if (listView.ItemsSource is IList<IconModel> sourceCollection) foreach (var item in selectedItems) sourceCollection.Remove(item);

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
    }

    private static void ListViewOnDragEnter(object sender, DragEventArgs e)
    {
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


        if (dropCommand.CanExecute(commandData))
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

    public record DropCommandData
    {
        public object sender { get; set; }
        public DragEventArgs e { get; set; }
    }
// todo
    public record NewBoxData
    {
        public Point Size { get; set; }
        public Point Position { get; set; }
        public string[]? Path { get; set; }
    }
    
    #region 辅助方法
    
    // 根据选择区域更新列表项选中状态
    private static void SelectItemsInArea(ListView listView, Rect area)
    {
        var scrollViewer = FindVisualChild<ScrollViewer>(listView);
        if (scrollViewer == null) return;

        // 检查是否处于多选模式
        bool isModifying = (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != 0;

        foreach (var item in listView.Items)
        {
            if (listView.ItemContainerGenerator.ContainerFromItem(item) is ListViewItem container)
            {
                // 计算列表项在滚动视图中的位置
                var itemTopLeft = container.TranslatePoint(new Point(0, 0), scrollViewer);
                var itemRect = new Rect(
                    itemTopLeft.X,
                    itemTopLeft.Y,
                    container.ActualWidth,
                    container.ActualHeight);

                // 判断是否与选择区域相交
                if (area.IntersectsWith(itemRect))
                {
                    container.IsSelected = true; // 选中
                }
                else if (!isModifying) // 非多选模式时取消选中
                {
                    container.IsSelected = false;
                }
            }
        }
    }

    // 在视觉树中查找指定类型的子元素
    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

        // 深度优先搜索
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result) return result;
            var descendant = FindVisualChild<T>(child);
            if (descendant != null) return descendant;
        }

        return null;
    }

    #endregion

    #region 装饰器类（实现选择框可视化）

    /// <summary>
    /// 自定义装饰器，用于在目标控件上绘制拖拽选择矩形
    /// 继承自Adorner以实现视觉层叠加效果
    /// </summary>
    private class DragSelectAdorner : Adorner
    {
        private readonly Canvas _canvas = new();// 容器
        private readonly Rectangle _selectionRect;// 选择矩形

        public DragSelectAdorner(UIElement adornedElement, Rectangle selectionRect) : base(adornedElement)
        {
            _selectionRect = selectionRect;         // 存储外部传入的选择矩形引用
            _canvas.Children.Add(_selectionRect);   // 将选择矩形添加到Canvas容器
            AddVisualChild(_canvas);                // 将Canvas加入视觉树，使其参与渲染
        }
        
        protected override int VisualChildrenCount => 1;                // 视觉子元素数量重写
        protected override Visual GetVisualChild(int index) => _canvas; // 获取指定索引的视觉子元素重写

        // 布局方法重写
        protected override Size ArrangeOverride(Size finalSize)
        {
            _canvas.Arrange(new Rect(finalSize));   // 让 _canvas 填充整个装饰器区域。
            return finalSize;                       // 维持原始尺寸不变，返回给布局系统
        }
    }

    #endregion
}