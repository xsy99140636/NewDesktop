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
    private ObservableCollection<BoxModel> _entities = new();
    
    [ObservableProperty]
    private ObservableCollection<IconModel> _icon = new();

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
            Name = $"盒子{Enumerable.OfType<BoxModel>(Entities).Count() + 1}"
        };
        Entities.Add(new BoxModel(shelf));
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
            Name = $"控件{Enumerable.OfType<IconModel>(Icon).Count() + 1}",
            Stock = Random.Shared.Next(0, 600),
        };
        Icon.Add(new IconModel(product));
    }

    /// <summary>
    /// 保存当前布局到JSON文件
    /// </summary>
    [RelayCommand]
    private void SaveLayout(string filePath)
    {
        // var data = new
        // {
        //     Shelves = Enumerable.OfType<BoxModel>(Entities).Select(s => s),
        //     Products = Enumerable.OfType<IconModel>(Entities).Select(p => p)
        // };
        // File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
    }
    
 #region 拖放处理（支持多选）
    
    public void DragOver(IDropInfo dropInfo)
    {
        bool isSingleValid = dropInfo.Data is IconModel || dropInfo.Data is Icon;
        bool isMultipleValid = dropInfo.Data is IEnumerable items && 
                             items.Cast<object>().All(x => x is IconModel);

        if (isSingleValid || isMultipleValid)
        {
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DestinationText = "放置到桌面";
            
            // 设置拖动时的偏移量（使图标跟随鼠标中心）
            // dropInfo.DragInfo.DragStartOffset = new Point(32, 32);

        }
    }
    
    /// <summary>
    /// 拖拽释放时的处理逻辑（完成数据转移）
    /// </summary>
    /// <param name="dropInfo">拖拽信息对象</param>
    public void Drop(IDropInfo dropInfo)
    {
        // 处理多选拖动
        if (dropInfo.Data is IconModel singleItem)
        {
            MoveItem(singleItem, dropInfo.DragInfo.SourceCollection);
            // 更新位置
            // UpdateItemPosition(item, dropInfo.DropPosition);
            singleItem.X = dropInfo.DropPosition.X - 32;  // 假设图标尺寸64x64，居中偏移
            singleItem.Y = dropInfo.DropPosition.Y - 32;
        }
        // 处理单选拖动
        else if (dropInfo.Data is IEnumerable multipleItems)
        {
            var offset = 0;
            // HandleMultipleItemsDrop(multipleItems.Cast<IconModel>(), dropInfo);
            foreach (IconModel item in multipleItems.Cast<IconModel>().ToList())
            {
                MoveItem(item, dropInfo.DragInfo.SourceCollection);
                // 更新位置
                // UpdateItemPosition(item, dropInfo.DropPosition);
                item.X = dropInfo.DropPosition.X - 32 - offset;  // 假设图标尺寸64x64，居中偏移
                item.Y = dropInfo.DropPosition.Y - 32 - offset;
                offset += 20;
            }
        }
    }

    /// <summary>
    /// 执行单个项目的移动操作
    /// </summary>
    /// <param name="item">要移动的图标对象</param>
    /// <param name="source">原始所属集合</param>
    private void MoveItem(IconModel item, IEnumerable source)
    {
        // 从源集合移除
        if (source is IList list) list.Remove(item);
        
        // 添加到目标集合
        if (!Icon.Contains(item)) Icon.Add(item);
    }
    
    #endregion
}