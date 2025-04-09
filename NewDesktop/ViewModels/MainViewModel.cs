using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
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
    private BoxModel? _selectedEntity;
    
    [ObservableProperty]
    private ObservableCollection<IconModel> _icons = new();
    
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

    [RelayCommand]
    private void RemoveShelf() => Entities.Remove(SelectedEntity);


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
            Name = $"控件{Enumerable.OfType<IconModel>(Icons).Count() + 1}",
            Stock = Random.Shared.Next(0, 600),
        };
        Icons.Add(new IconModel(product));
    }

    
    #region 保存加载
    
    [RelayCommand]
    private void ExportLayout()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "JSON 文件 (*.json)|*.json",
            DefaultExt = ".json",
            FileName = "桌面布局.json"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            SaveLayout(saveFileDialog.FileName);
        }
    }

    [RelayCommand]
    private void ImportLayout()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON 文件 (*.json)|*.json",
            DefaultExt = ".json"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            LoadLayout(openFileDialog.FileName);
        }
    }
    
    /// <summary>
    /// 保存当前布局到JSON文件
    /// </summary>
    [RelayCommand]
    private void SaveLayout(string path)
    {
        try
        {
            var data = new
            {
                Boxes = Entities.Select(b => b.Model),
                Icons = Icons.Select(i => i.Model)
            };

            string json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            File.WriteAllText(path, json);

            // 显示成功消息
            // MessageBox.Show("布局保存成功!", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void LoadLayout(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<dynamic>(json);

            // 清空当前数据
            Entities.Clear();
            Icons.Clear();

            // 加载盒子
            if (data.Boxes != null)
            {
                foreach (var boxJson in data.Boxes)
                {
                    var box = JsonConvert.DeserializeObject<Box>(boxJson.ToString());
                    Entities.Add(new BoxModel(box));
                }
            }

            // 加载图标
            if (data.Icons != null)
            {
                foreach (var iconJson in data.Icons)
                {
                    var icon = JsonConvert.DeserializeObject<Icon>(iconJson.ToString());
                    Icons.Add(new IconModel(icon));
                }
            }
            
            // MessageBox.Show("布局加载成功!", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
    }
    
    #endregion
    
    
    
    [RelayCommand]
    private void OpenBoxSettings()
    {
        var settingsWindow = new SettingsWindow(this);
        settingsWindow.ShowDialog();
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
        if (!Icons.Contains(item)) Icons.Add(item);
    }
    
    #endregion
}