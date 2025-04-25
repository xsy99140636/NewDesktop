using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewDesktop.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using NewDesktop.Shell;
using System.Windows.Media;
using NewDesktop.Services;
using static NewDesktop.Behaviors.IconDragDropBehavior;

namespace NewDesktop.ViewModels;

/// <summary>
/// 桌面盒子视图模型
/// </summary>
public partial class BoxModel : ObservableObject// , IDropTarget
{
    [ObservableProperty]
    private Box _model;

    [ObservableProperty]
    private ObservableCollection<IconModel> _iconModels = new();

    // partial void OnIconChanged(ObservableCollection<IconModel> value)
    // {
    //     Icons = _icon.Model;
    // }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Height))]
    private double _height1;

    #region 属性绑定

    // public ObservableCollection<Icon> Icons
    // {
    //     get => Model.Icons;
    //     set => SetProperty(Model.Icons, value, Model, (m, v) => m.Icons = v);
    // }

    public double X
    {
        get => Model.X;
        set => SetProperty(Model.X, value, Model, (m, v) => m.X = v);
    }

    public double Y
    {
        get => Model.Y;
        set => SetProperty(Model.Y, value, Model, (m, v) => m.Y = v);
    }
    
    public double Height
    {
        get => Model.Height;
        set
        {
            if (SetProperty(Model.Height, value, Model, (m, v) => m.Height = v))
            {
                Height1 = value; // 同步更新Height1
            }
        }
    }
    
    public double Width
    {
        get => Model.Width;
        set => SetProperty(Model.Width, value, Model, (m, v) => m.Width = v);
    }
    
    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
    }

    public double HeadHeight
    {
        get => Model.HeadHeight;
        set
        {
            if (!SetProperty(Model.HeadHeight, value, Model, (m, v) => m.HeadHeight = v)) return;
            if (IsExpanded == false)
            {
                Height1 = value; // 同步更新Height1
            }
        }
    }

    public int Corner
    {
        get => Model.Corner;
        set => SetProperty(Model.Corner, value, Model, (m, v) => m.Corner = v);
    }
    
    public Color BoxColor
    {
        get => Model.BoxColor;
        set => SetProperty(Model.BoxColor, value, Model, (m, v) => m.BoxColor = v);
    }
    
    public bool? IsExpanded
    {
        get => Model.IsExpanded;
        set => SetProperty(Model.IsExpanded, value, Model, (m, v) => m.IsExpanded = v);
    }
    
    #endregion
    
    public BoxModel(Box model)
    {
        _model = model;

        foreach (var product in _model.Icons)
        {
            var iconModel = new IconModel(product);
            // iconModel.JumboIcon = IconExtractor.GetIcon(iconModel.Path);
            iconModel.JumboIcon = IconGet.GetThumbnail(iconModel.Path);
            IconModels.Add(iconModel);
            //Icon.Add(new IconModel(product));
        }
        if (IsExpanded == true) 
        {
            _height1 = model.Height; // 初始化 Height1
        }
        else 
        {
            _height1 = model.HeadHeight; 
        }
    }

    [RelayCommand]
    private void RightClick(IList selectedItems)
    {
        if (selectedItems == null) return;

        // 转换为 IconModel 并过滤有效路径
        var items = selectedItems
            .Cast<IconModel>()
            .Where(icon => !string.IsNullOrEmpty(icon.Path) && (File.Exists(icon.Path) || Directory.Exists(icon.Path)))
            .Select(i => i.Path)
            .ToArray(); // 关键转换：IEnumerable<string> => string[]

        if (items.Length == 0) return;

        // 获取鼠标屏幕坐标
        // if (!GetCursorPos(out POINT cursorPos)) return;
        // 使用更现代的坐标获取方式
        var mousePosition = System.Windows.Forms.Control.MousePosition;
        var mousePoint = new Point(mousePosition.X, mousePosition.Y);
        
        // 获取窗口句柄
        // 获取主窗口句柄
        var mainWindow = Application.Current.MainWindow; // 修复此处
        var handle = new WindowInteropHelper(mainWindow).Handle;

        // 显示系统右键菜单
        DesktopAttacher.ShowContextMenu(items, mousePoint, handle);
    }
    
    
    #region 拖动处理
    
    //[RelayCommand]
    //private void HandleDrop(object droppedFiles)
    //{
    //    // 处理接收到的文件路径
    //    //foreach (var file in droppedFiles)
    //    //{
    //    //    var fileName = Path.GetFileNameWithoutExtension(file);
    //    //    var newIcon = new Icon
    //    //    {
    //    //        X = Random.Shared.Next(50, 1000),
    //    //        Y = Random.Shared.Next(50, 600),
    //    //        Name = fileName,
    //    //        Path = file,
    //    //        Stock = 1,
    //    //    };
    //    //    IconModels.Add(new(newIcon));
    //    //}
    //}
    [RelayCommand]
    private void HandleDrop(DropCommandData dropData)
    {
        // 处理文件拖放逻辑
        if (dropData.e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])dropData.e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                var product = new Icon
                {
                    X = 0,
                    Y = 0,
                    Name = Path.GetFileNameWithoutExtension(file),
                    Path = file,
                    
                    // Stock = Random.Shared.Next(0, 600),
                };
                
                var iconModel = new IconModel(product)
                {
                    //JumboIcon = IconExtractor.GetIcon(filePath)
                    JumboIcon = IconGet.GetThumbnail(file)
                };
                
                IconModels.Add(iconModel); // 添加到主集合
            }
        }
    }
    
    #endregion
}