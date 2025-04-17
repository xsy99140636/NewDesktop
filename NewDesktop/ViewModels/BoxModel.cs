using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using NewDesktop.Models;
using NewDesktop.Module;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;

namespace NewDesktop.ViewModels;

/// <summary>
/// 桌面盒子视图模型
/// </summary>
public partial class BoxModel : ObservableObject, IDropTarget
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

    public ObservableCollection<Icon> Icons
    {
        get => Model.Icons;
        set => SetProperty(Model.Icons, value, Model, (m, v) => m.Icons = v);
    }

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
            if (IsExpanded == true)
            {
                Height1 = value; // 同步更新Height1
            }
        }
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

    #region 拖动处理

    /// <summary>
    /// 拖拽悬停时触发的逻辑（验证拖拽数据有效性）
    /// </summary>
    /// <param name="dropInfo">拖拽信息对象，包含拖拽数据和上下文</param>
    public void DragOver(IDropInfo dropInfo)
    {
        // 验证拖拽数据是否符合要求：
        // 1. 单个 IconModel 对象
        // 2. 或多个 IconModel 组成的集合
        bool validData = dropInfo.Data is IconModel
                         || (dropInfo.Data is IEnumerable items && items.Cast<object>().All(x => x is IconModel));
    
        if (validData)
        {
            // 设置拖拽效果为移动操作
            dropInfo.Effects = DragDropEffects.Move;
            // 显示友好提示文本
            dropInfo.DestinationText = $"放入{Name}";
        }
    }

    /// <summary>
    /// 拖拽释放时的处理逻辑（完成数据转移）
    /// </summary>
    /// <param name="dropInfo">拖拽信息对象</param>
    public void Drop(IDropInfo dropInfo)
    {
        // 处理单个项目拖拽
        if (dropInfo.Data is IconModel singleItem)
        {
            MoveItem(singleItem, dropInfo.DragInfo.SourceCollection);
        }
        // 处理多选项目拖拽
        else if (dropInfo.Data is IEnumerable multipleItems)
        {
            // 转换为具体对象列表（避免迭代时修改集合的问题）
            foreach (IconModel item in multipleItems.Cast<IconModel>().ToList())
            {
                MoveItem(item, dropInfo.DragInfo.SourceCollection);
            }
        }
    }

    /// <summary>
    /// 执行单个项目的移动操作
    /// </summary>
    /// <param name="item">要移动的图标对象</param>
    /// <param name="source">原始所属集合</param>
    /// <summary>
    /// 执行单个项目的移动操作
    /// </summary>
    /// <param name="item">要移动的图标对象</param>
    /// <param name="source">原始所属集合</param>
    private void MoveItem(IconModel item, IEnumerable source)
    {
        // 从源集合中移除（仅当源支持修改时）
        if (source is IList list) list.Remove(item);
        IconModels.Add(item);
        // 避免重复添加至目标集合
        if (!IconModels.Contains(item)) 
        {
            // 添加至当前集合
            
            // 同步更新底层数据模型
            Model.Icons.Add(item.Model);
        }
    }

    #endregion
}