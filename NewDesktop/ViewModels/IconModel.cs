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
    // private readonly Product _model;
    [ObservableProperty]
    private Icon _model;
    
    public string Name
    {
        get => Model.Name;
        set => SetProperty<Icon, string>(Model.Name, value, Model, (m, v) => m.Name = v);
    }

    public int Stock
    {
        get => Model.Stock;
        set => SetProperty<Icon, int>(Model.Stock, value, Model, (m, v) => m.Stock = v);
    }

    public double X
    {
        get => Model.X;
        set => SetProperty<Icon, double>(Model.X, value, Model, (m, v) => m.X = v);
    }

    public double Y
    {
        get => Model.Y;
        set => SetProperty<Icon, double>(Model.Y, value, Model, (m, v) => m.Y = v);
    }
        
    // public ObservableCollection<object> ParentCollection 
    // {
    //     get => ParentCollection;
    //     set => SetProperty(ParentCollection, value, Model, (m, v) => m.ParentCollection = v);
    // }
        
    // 添加父集合维护逻辑
    [ObservableProperty]
    private ObservableCollection<object> _parentCollection;

    // 构造函数增加父集合参数
    public IconModel(Icon model, ObservableCollection<object> parent = null)
    {
        _model = model;
        _parentCollection = parent;
    }
        
    public IconModel(Icon model)
    {
        _model = model;
    }

    // 开始拖动时传递数据
    public void StartDrag(IDragInfo dragInfo)
    {
        dragInfo.Data = this; // 传递数据模型.Model
        dragInfo.Effects = DragDropEffects.Move;
    }

    // 修改DragDrop完成回调
    public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
    {
        if (operationResult == DragDropEffects.Move && 
            dragInfo.Data is IconModel movedItem)
        {
            // 从原父集合中移除
            movedItem.ParentCollection?.Remove(movedItem);
        }
    }
    
    public bool CanStartDrag(IDragInfo dragInfo) => true;

    // 拖动取消时的逻辑（如果需要）
    public void Dropped(IDropInfo dropInfo) { }

    // 拖动取消时的逻辑（如果需要） 
    public void DragCancelled() { }

    // 异常处理
    public bool TryCatchOccurredException(Exception exception) => false;

}