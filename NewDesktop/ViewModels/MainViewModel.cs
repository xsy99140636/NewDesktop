using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewDesktop.Models;
using NewDesktop.Services;
using NewDesktop.Shell;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using static NewDesktop.Behaviors.IconDragDropBehavior;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace NewDesktop.ViewModels;

/// <summary>
/// 主视图模型 - 管理系统所有实体
/// </summary>
public partial class MainViewModel : ObservableObject//, IDropTarget
{
    [ObservableProperty]
    private ObservableCollection<BoxModel> _entities = new();
    
    [ObservableProperty]
    private BoxModel? _selectedEntity;
    
    [ObservableProperty]
    private ObservableCollection<IconModel> _icons = new();

    private readonly string _defaultLayoutPath = "桌面布局.json";

    public MainViewModel()
    {
        // 初始化时尝试加载默认布局
         InitializeLayout();
         布局初始化();
    }

    [RelayCommand]
    private void NewBox(NewBoxData selectedItems)
    {
        Debug.WriteLine($"{selectedItems.Size.X}, {selectedItems.Size.Y}");
        Debug.WriteLine($"{selectedItems.Position.X}, {selectedItems.Position.Y}");
        var shelf = new Box
        {
            X = selectedItems.Position.X,
            Y = selectedItems.Position.Y,
            Height = selectedItems.Size.X,
            Width = selectedItems.Size.Y,
            Name = $"盒子{Enumerable.OfType<BoxModel>(Entities).Count() + 1}",
        };
        Entities.Add(new BoxModel(shelf,this));
    }

    [RelayCommand]
    private void RightClick(IList selectedItems)
    {
        if (selectedItems == null) return;

        // 转换为 IconModel 并过滤有效路径
        var items = selectedItems
            .Cast<IconModel>()
            .Where(icon => !string.IsNullOrEmpty(icon.Path) && (File.Exists(icon.Path) || Directory.Exists(icon.Path)))
            .Select(i => i.Path)
            .ToArray(); // 关键转换：IEnumerable<string> => string[]

        if (items.Length == 0) return;

        // 获取鼠标屏幕坐标
        // if (!GetCursorPos(out POINT cursorPos)) return;
        // 使用更现代的坐标获取方式
        var mousePosition = System.Windows.Forms.Control.MousePosition;
        var mousePoint = new Point(mousePosition.X, mousePosition.Y);
        
        // 获取窗口句柄
        // 获取主窗口句柄
        var mainWindow = Application.Current.MainWindow; // 修复此处
        var handle = new WindowInteropHelper(mainWindow).Handle;

        // 显示系统右键菜单
        DesktopAttacher.ShowContextMenu(items, mousePoint, handle);
    }
    
    
    /// <summary>
    /// 初始化布局 - 加载保存的布局并同步桌面文件
    /// </summary>
    private void InitializeLayout()
    {
        
        try
        {
            // 如果存在保存的布局则加载
            if (File.Exists(_defaultLayoutPath))
            {
                LoadLayout(_defaultLayoutPath);
            }

            // 获取桌面文件
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            // var desktopFiles = Directory.GetFiles(desktopPath).ToList();
            var desktopFiles = Directory.GetFileSystemEntries(desktopPath).ToList();
            //.Where(f => !f.EndsWith(".lnk")) // 排除快捷方式
            //.ToList()

            // 同步处理
            SyncWithDesktopFiles(desktopFiles);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"初始化布局失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 同步桌面文件与现有图标数据
    /// </summary>
    private void SyncWithDesktopFiles(List<string> desktopFiles)
    {
        try
        {
            // 1. 合并所有图标来源（主集合+所有盒子）
            var allIcons = Icons
                .Concat(Entities.SelectMany(box => box.IconModels))
                .ToList();

            // 2. 处理已删除文件（从所有位置移除）
            // var iconsToRemove = allIcons
            //     .Where(i => string.IsNullOrEmpty(i.Path))
            //     .GroupBy(i => i.Path, StringComparer.OrdinalIgnoreCase)
            //     .Where(g => !File.Exists(g.Key) && !Directory.Exists(g.Key))
            //     .SelectMany(g => g)  // 展开所有匹配的组
            //     .ToList();


            // 1. 统计所有非空Path的出现次数（忽略大小写）
            var pathCounts = allIcons
                // 过滤掉Path为null或空字符串
                .Where(i => !string.IsNullOrEmpty(i.Path))
                // 按路径分组（忽略大小写）
                .GroupBy(i => i.Path, StringComparer.OrdinalIgnoreCase)
                // 转为字典：路径 -> 出现次数
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
            
            var iconsToRemove = allIcons
                .Where(i =>
                    // 条件1: Path为null或空字符串
                    string.IsNullOrEmpty(i.Path) ||
                    // 条件2: 路径对应的文件/目录不存在
                    (!File.Exists(i.Path!) && !Directory.Exists(i.Path!)) ||
                    // 条件3: 路径重复
                    (pathCounts.TryGetValue(i.Path, out var count) && count > 1)
                )
                .ToList();
            
            
            foreach (var icon in iconsToRemove)
            {
                // 从主集合移除
                if (Icons.Contains(icon)) Icons.Remove(icon);
            
                // 从所属盒子移除
                foreach (var box in Entities.Where(b => b.IconModels.Contains(icon))) box.IconModels.Remove(icon);
                // box.Model.Products.Remove(icon.Model);
            }

            // 3. 创建现有路径集合（包含所有有效图标）
            var existingPaths = allIcons
                .Except(iconsToRemove)
                .Select(i => i.Path)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 4. 添加新文件图标
            foreach (var filePath in desktopFiles)
            {
                if (!existingPaths.Contains(filePath))
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var newIcon = new Icon
                    {
                        X = Random.Shared.Next(50, 1000),
                        Y = Random.Shared.Next(50, 600),
                        Name = fileName,
                        Path = filePath,
                        Stock = 1,
                    };

                    var iconModel = new IconModel(newIcon)
                    {
                        //JumboIcon = IconExtractor.GetIcon(filePath)
                         JumboIcon = IconGet.GetThumbnail(filePath)
                    };

                    Icons.Add(iconModel); // 添加到主集合
                }
            }

            SaveLayout(_defaultLayoutPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"同步失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

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
            Name = $"盒子{Enumerable.OfType<BoxModel>(Entities).Count() + 1}",
        };
        Entities.Add(new BoxModel(shelf,this));
    }

    [RelayCommand]
    private void RemoveShelf(BoxModel? Selected)
    {
        Selected ??= SelectedEntity;
        Entities.Remove(Selected);
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
            Name = $"控件{Enumerable.OfType<IconModel>(Icons).Count() + 1}",
            Stock = Random.Shared.Next(0, 600),
        };
        Icons.Add(new IconModel(product));
    }
    
    #region 打开设置
    [RelayCommand]
    private void OpenBoxSettings()
    {
        var settingsWindow = new SettingsWindow(this);
        settingsWindow.ShowDialog();
    }
    #endregion

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

        if (saveFileDialog.ShowDialog() == true) SaveLayout(saveFileDialog.FileName);
    }

    [RelayCommand]
    private void ImportLayout()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON 文件 (*.json)|*.json",
            DefaultExt = ".json"
        };

        if (openFileDialog.ShowDialog() == true) SaveLoadService.LoadLayout(openFileDialog.FileName, Entities, Icons,this);

        SaveLayout(_defaultLayoutPath);
    }

    /// <summary>
    /// 保存当前布局到JSON文件
    /// </summary>
    [RelayCommand]
    private void SaveLayout(string path) => SaveLoadService.SaveLayout(path, Entities, Icons);
    
    /// <summary>
    /// 加载JSON文件到当前布局
    /// </summary>
    [RelayCommand]
    private void LoadLayout(string path) => SaveLoadService.LoadLayout(path, Entities, Icons,this);

    #endregion
        
    #region 拖放处理
    
    [RelayCommand]
    private void HandleDrop(DropCommandData dropData)
    {
        // if (dropData.sender is not ListView listView) return;
        if (dropData.sender is not ListView targetListView) return;
        if (!dropData.e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        
        // 查找目标ListView内部的Canvas
        var canvas = FindVisualChild<Canvas>(targetListView);
        if (canvas == null) return;

        // 获取相对于Canvas的鼠标位置
        Point dropPosition = dropData.e.GetPosition(canvas);

        var dropPositionX = Math.Round((dropPosition.X-32)/64)*64;
        var dropPositionY = Math.Round((dropPosition.Y-40)/80)*80;
        
        Debug.WriteLine($"{dropPositionX}");
        
        // 处理文件拖放逻辑
        if (dropData.e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])dropData.e.Data.GetData(DataFormats.FileDrop);

            查找空位(2,2,files);
            foreach (var file in files)
            {
                var product = new Icon
                {
                    X = dropPositionX,
                    Y = dropPositionY,
                    Name = Path.GetFileNameWithoutExtension(file),
                    Path = file,
                    
                    // Stock = Random.Shared.Next(0, 600),
                };
                
                var iconModel = new IconModel(product)
                {
                    //JumboIcon = IconExtractor.GetIcon(filePath)
                    JumboIcon = IconGet.GetThumbnail(file)
                };

                Icons.Add(iconModel); // 添加到主集合
            }
        }
    }
    
    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result) return result;
            else
            {
                var descendant = FindVisualChild<T>(child);
                if (descendant != null) return descendant;
            }
        }
        return null;
    }
    
    #endregion


    private double 高=80;
    private double 宽=64;
    private IconModel[]? 测试;
    
    void 布局初始化()
    {
        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;
        
        Debug.WriteLine($"<UNK>{screenWidth}*{screenHeight}");
        
        测试 = new IconModel[30];
        测试[16] = new IconModel(new Icon());
        测试[18] = new IconModel(new Icon());
        测试[19] = new IconModel(new Icon());
        索引计算(96, 160);
        查找空位(96, 160,["a","b"]);
    }


    int 索引计算(double x, double y)
    {
        int 行 = (int)((y - 高 / 2) / 高);
        int 列 = (int)((x - 宽 / 2) / 宽);

        int 最大行 = (int)Math.Round(SystemParameters.PrimaryScreenHeight / 高);
        
        int 索引 = (int)(列 * Math.Round(SystemParameters.PrimaryScreenHeight / 高) + 行);
        
        Debug.WriteLine($"<UNK>{索引}");
        
        return 索引;
        
    }

    void 查找空位(double x, double y,string[] 路径)
    {
        var 索引 = 索引计算(x,y);
        
        foreach (var file in 路径)
        {
            for (int i = 索引; i < 30; i++)
            {
                if (测试[i] == null)
                {
                    var product = new Icon
                    {
                        X = 22,
                        Y = 22,
                        Name = Path.GetFileNameWithoutExtension(file),
                        Path = file,

                        // Stock = Random.Shared.Next(0, 600),
                    };

                    var iconModel = new IconModel(product)
                    {
                        //JumboIcon = IconExtractor.GetIcon(filePath)
                        JumboIcon = IconGet.GetThumbnail(file)
                    };

                    测试[i] = iconModel;

                    索引 = i+1;
                    
                    break;
                }
            }
            Debug.WriteLine($"<UNK>{测试}");
        }
        
    }
}