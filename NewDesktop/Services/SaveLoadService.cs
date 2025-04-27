using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using NewDesktop.Models;
using NewDesktop.ViewModels;
using Newtonsoft.Json;

namespace NewDesktop.Services;

/// <summary>
/// 布局保存与加载服务类
/// 职责：处理桌面布局的序列化、反序列化及数据清理工作
/// </summary>
abstract class SaveLoadService
{
    /// <summary>
    /// 将当前布局保存到JSON文件
    /// </summary>
    /// <param name="path">源文件路径</param>
    /// <param name="boxModels">目标盒子集合</param>
    /// <param name="iconModels">目标图标集合</param>
    /// <remarks>
    /// 执行流程：
    /// 1. 准备数据模型
    /// 2. 序列化为JSON
    /// 3. 写入文件系统
    /// </remarks>
    public static void SaveLayout(string path, ObservableCollection<BoxModel> boxModels, ObservableCollection<IconModel> iconModels)
    {
        try
        {
            // 获取当前所有盒子实体
            var ee = boxModels;

            // 同步盒子模型与底层数据模型
            foreach (var boxModel in ee)
            {
                /* 清空盒子原有图标集合
                 * 注意：这里操作的是BoxModel.Model.Icons而不是BoxModel.IconModels
                 * 因为最终需要保存的是数据模型(Box)而非视图模型(BoxModel) */
                boxModel.Model.Icons.Clear();

                // 将视图模型中的图标同步到数据模型
                foreach (var iconModel in boxModel.IconModels) boxModel.Model.Icons.Add(iconModel.Model);
            }

            // 构建待序列化的匿名对象
            var data = new
            {
                // 序列化所有盒子的数据模型
                Boxes = ee.Select(b => b.Model),
                // 序列化主图标集合的数据模型
                Icons = iconModels.Select(i => i.Model)
            };

            /* JSON序列化配置：
             * 1. Formatting.Indented 美化输出（带缩进）
             * 2. 忽略循环引用（防止模型间相互引用导致序列化失败） */
            string json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // 写入文件系统（会覆盖已存在文件）
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            // 统一错误处理：显示友好错误信息
            MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    /// <summary>
    /// 从JSON文件加载布局
    /// </summary>
    /// <param name="path">源文件路径</param>
    /// <param name="boxModels">目标盒子集合</param>
    /// <param name="iconModels">目标图标集合</param>
    /// <remarks>
    /// 执行流程：
    /// 1. 读取并解析JSON
    /// 2. 重建视图模型
    /// 3. 执行数据清理
    /// </remarks>
    public static void LoadLayout(string path, ObservableCollection<BoxModel> boxModels, ObservableCollection<IconModel> iconModels, MainViewModel parent)
    {
        try
        {
            // 读取JSON文件内容
            var json = File.ReadAllText(path);

            // 动态解析JSON（不定义具体类型，使用dynamic）
            var data = JsonConvert.DeserializeObject<dynamic>(json);

            /* 清空现有集合
             * 注意：这里直接操作传入的参数集合
             * 保证与调用方的数据同步 */
            boxModels.Clear();
            iconModels.Clear();

            // 加载盒子数据
            if (data?.Boxes != null) // 防御性检查
            {
                foreach (var boxJson in data.Boxes)
                {
                    // 反序列化为Box数据模型
                    var box = JsonConvert.DeserializeObject<Box>(boxJson.ToString());

                    // 创建对应的视图模型并添加到集合
                    boxModels.Add(new BoxModel(box,parent));
                }
            }

            // 加载图标数据
            if (data?.Icons != null) // 防御性检查
            {
                foreach (var iconJson in data.Icons)
                {
                    // 反序列化为Icon数据模型
                    var icon = JsonConvert.DeserializeObject<Icon>(iconJson.ToString());

                    // 创建视图模型并加载缩略图
                    var iconModel = new IconModel(icon)
                    
                    {
                        // JumboIcon = IconExtractor.GetIcon(icon.Path)
                         JumboIcon = IconGet.GetThumbnail(icon.Path)
                    };

                    iconModels.Add(iconModel);
                }
            }

            // 后处理：清理无效图标
            CleanupInvalidIcons(boxModels, iconModels);
        }
        catch (Exception ex)
        {
            // 统一错误处理：显示友好错误信息
            MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 清理无效图标
    /// </summary>
    /// <param name="boxModels">盒子集合</param>
    /// <param name="iconModels">主图标集合</param>
    /// <remarks>
    /// 清理规则：
    /// 1. 路径为空的图标
    /// 2. 指向不存在的文件/目录的图标
    /// 3. 路径重复的图标（保留第一个出现的）
    /// </remarks>
    private static void CleanupInvalidIcons(ObservableCollection<BoxModel> boxModels, ObservableCollection<IconModel> iconModels)
    {
        /* 合并所有图标来源：
         * 1. 主图标集合(icons)
         * 2. 所有盒子中的图标(boxes.SelectMany) */
        var allIcons = iconModels
            .Concat(boxModels.SelectMany(b => b.IconModels))
            .ToList();

        /* 统计有效路径的出现次数（忽略大小写）
         * 数据结构：Dictionary<路径, 出现次数> */
        var pathCounts = allIcons
            // 过滤无效路径
            .Where(i => !string.IsNullOrEmpty(i.Path))
            // 按路径分组（不区分大小写）
            .GroupBy(i => i.Path, StringComparer.OrdinalIgnoreCase)
            // 转换为字典（保留大小写不敏感的查找特性）
            .ToDictionary(
                g => g.Key, // 键：原始路径
                g => g.Count(), // 值：出现次数
                StringComparer.OrdinalIgnoreCase);

        // 识别需要移除的图标
        var iconsToRemove = allIcons
            .Where(i =>
                // 条件1：路径无效（null或空字符串）
                string.IsNullOrEmpty(i.Path) ||
                // 条件2：文件系统不存在该路径
                (!File.Exists(i.Path) && !Directory.Exists(i.Path)) ||
                // 条件3：路径重复（出现次数>1）
                (pathCounts.TryGetValue(i.Path, out var count) && count > 1)
            )
            .ToList();

        // 执行移除操作
        foreach (var icon in iconsToRemove)
        {
            // 从主集合移除
            if (iconModels.Contains(icon)) iconModels.Remove(icon);

            // 从所有包含该图标的盒子中移除
            foreach (var box in boxModels.Where(b => b.IconModels.Contains(icon))) box.IconModels.Remove(icon);
        }
    }
}