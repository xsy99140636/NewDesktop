using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using NewDesktop.Models;

namespace NewDesktop.ViewModels;

/// <summary>
/// 主视图模型 - 管理系统所有实体
/// </summary>
public partial class MainViewModel : ObservableObject, IDropTarget
{
    [ObservableProperty]
    private ObservableCollection<NewDesktop.ViewModels.BoxModel> _entities = new();
    
    [ObservableProperty]
    private ObservableCollection<NewDesktop.ViewModels.IconModel> _entities1 = new();

    /// <summary>
    /// 添加新盒子到随机位置
    /// </summary>
    [RelayCommand]
    private void AddShelf()
    {
        var shelf = new Box
        {
            X = Random.Shared.Next(0, 800),
            Y = Random.Shared.Next(0, 600),
            Name = $"盒子{Enumerable.OfType<NewDesktop.ViewModels.BoxModel>(Entities).Count() + 1}"
        };
        Entities.Add(new NewDesktop.ViewModels.BoxModel(shelf));
    }

    /// <summary>
    /// 添加新图标到随机位置
    /// </summary>
    [RelayCommand]
    private void AddProduct()
    {
        var product = new Icon
        {
            X = Random.Shared.Next(0, 800),
            Y = Random.Shared.Next(0, 600),
            Name = $"控件{Enumerable.OfType<NewDesktop.ViewModels.IconModel>(Entities1).Count() + 1}",
            Stock = Random.Shared.Next(0, 600),
        };
        Entities1.Add(new NewDesktop.ViewModels.IconModel(product));
    }

    /// <summary>
    /// 保存当前布局到JSON文件
    /// </summary>
    [RelayCommand]
    private void SaveLayout(string filePath)
    {
        var data = new
        {
            Shelves = Enumerable.OfType<NewDesktop.ViewModels.BoxModel>(Entities).Select(s => s),
            Products = Enumerable.OfType<NewDesktop.ViewModels.IconModel>(Entities).Select(p => p)
        };
        File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
    }
        
    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is NewDesktop.ViewModels.IconModel || dropInfo.Data is Icon)
        {
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DestinationText = "放置到桌面";
        }
    }

// MainViewModel.cs
    public void Drop(IDropInfo dropInfo)
    {
        if (dropInfo.Data is NewDesktop.ViewModels.IconModel iconModel)
        {
            // 显式从源集合移除
            if (dropInfo.DragInfo.SourceCollection is IList source)
            {
                source.Remove(iconModel);
            }

            // 直接使用现有实例
            iconModel.X = dropInfo.DropPosition.X;
            iconModel.Y = dropInfo.DropPosition.Y;
            Entities1.Add(iconModel);
        }
    }
}