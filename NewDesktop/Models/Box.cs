using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NewDesktop.Models;

/// <summary>
/// 盒子数据模型
/// </summary>
public partial class Box : PositionedObject
{    
    /// <summary>
    /// 盒子名称
    /// </summary>
    [ObservableProperty]
    private string _name = "盒子";    
    
    /// <summary>
    /// 标题栏高度
    /// </summary>
    [ObservableProperty]
    private double _headHeight = 24;
    
    /// <summary>
    /// 是否折叠
    /// </summary>
    [ObservableProperty]
    private bool? _isExpanded = true;
    
    /// <summary>
    /// 子项数据
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Icon> _icons = [];

}