using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NewDesktop.Models;

/// <summary>
/// 盒子数据模型
/// </summary>
public partial class Box : PositionedObject
{
    // 子项数据
    [ObservableProperty]
    private ObservableCollection<Icon> _products = [];

    [ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(DisplayName))] // 当名称变化时通知显示名称
    private string _name = "盒子";

}