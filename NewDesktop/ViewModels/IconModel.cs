using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using GongSolutions.Wpf.DragDrop;
using NewDesktop.Models;

namespace NewDesktop.ViewModels;

/// <summary>
///图标视图模型
/// </summary>
public partial class IconModel : ObservableObject, IDragSource
{

    [ObservableProperty]
    private Icon _model;

    #region 属性绑定

    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
    }
    
    public string Path
    {
        get => Model.Path;
        set => SetProperty(Model.Path, value, Model, (m, v) => m.Path = v);
    }
    
    public int Stock
    {
        get => Model.Stock;
        set => SetProperty(Model.Stock, value, Model, (m, v) => m.Stock = v);
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

    #endregion
    
    // 构造函数增加父集合参数
    public IconModel(Icon model, ObservableCollection<object> parent = null)
    {
        _model = model;
    }
        
    // public IconModel(Icon model)
    // {
    //     _model = model;
    // }


    #region 拖动

    // 开始拖动时的初始化操作
    public void StartDrag(IDragInfo dragInfo)
    {
        // 获取所有选中的项（支持多选）
        var selectedItems = dragInfo.SourceItems?.Cast<IconModel>().ToList() 
                            ?? new List<IconModel> { this };
        // 传递选中的集合
        dragInfo.Data = selectedItems.Count == 1 ? 
            selectedItems.First() : 
            selectedItems.AsEnumerable();
        
        dragInfo.Effects = DragDropEffects.Move;
        // dragInfo.Data = this; // 传递数据模型.Model
        // dragInfo.Effects = DragDropEffects.Move;
    }
    
    // 判断是否允许启动拖动操作（这里始终允许）
    public bool CanStartDrag(IDragInfo dragInfo) => true;

    // 修改DragDrop完成回调
    public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
    {
        // if (operationResult == DragDropEffects.Move && 
        //     dragInfo.Data is IconModel movedItem)
        // {
        //     // 从原父集合中移除
        //     movedItem.ParentCollection?.Remove(movedItem);
        // }
    }
    
    // 当元素被放置到目标位置时的处理
    public void Dropped(IDropInfo dropInfo) { }

    // 拖动操作被取消时的处理
    public void DragCancelled() { }

    // 异常处理策略
    public bool TryCatchOccurredException(Exception exception) => false;

    #endregion

}