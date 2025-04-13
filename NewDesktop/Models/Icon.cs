using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NewDesktop.Models;

/// <summary>
/// 图标数据模型
/// </summary>
public partial class Icon : PositionedObject
{
    [ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(DisplayName))] // 当名称变化时通知显示名称
    private string _name = "未命名商品";

    [ObservableProperty]
    private int _stock = 200;
    
    [ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(DisplayName))] // 当名称变化时通知显示名称
    private string _path = "";
    
}