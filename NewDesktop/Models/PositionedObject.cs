using CommunityToolkit.Mvvm.ComponentModel;

namespace NewDesktop.Models;

/// <summary>
/// 数据模型基类
/// </summary>
public abstract partial class PositionedObject: ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();
        
    [ObservableProperty]
    private double _x;
        
    [ObservableProperty]
    private double _y;
        
    [ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(Size))] // 尺寸变化时通知Size属性
    private double _width = 400;

    [ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(Size))]
    private double _height = 300;
        
    // 示例计算属性
    //public System.Windows.Size Size => new(Width, Height);
    // PositionedObject.cs
}