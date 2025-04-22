using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;

namespace NewDesktop.Services;

public static class IconGet
{
    public static BitmapSource GetThumbnail(string path, ShellIconSize size = ShellIconSize.ExtraLarge)
    {
        try
        {
            // 使用 ShellObject 代替 ShellFile，支持文件和文件夹
            var thumbnail = ShellObject.FromParsingName(path).Thumbnail;
            // var thumbnail = shellObject;

            thumbnail.FormatOption = ShellThumbnailFormatOption.Default;

            var originalSize = thumbnail.BitmapSource.Width;
            System.Diagnostics.Debug.WriteLine($"缩略图:{originalSize}");
            
            thumbnail.AllowBiggerSize = true;
            thumbnail.CurrentSize = GetIconSize(size);
            
            return thumbnail.BitmapSource;
        }
        catch (Exception ex)
        {
            // 处理异常（如路径无效、无权限等）
            Console.WriteLine($"Error loading thumbnail: {ex.Message}");
            return null; // 或返回一个默认图标
        }
    }
    
    private static System.Windows.Size GetIconSize(ShellIconSize size)
    {
        return size switch
        {
            ShellIconSize.Small => new System.Windows.Size(16, 16),
            ShellIconSize.Medium => new System.Windows.Size(32, 32),
            ShellIconSize.Large => new System.Windows.Size(48, 48),
            ShellIconSize.ExtraLarge => new System.Windows.Size(256, 256),
            ShellIconSize.Jumbo => new System.Windows.Size(1024, 1024),
            _ => new System.Windows.Size(512, 512)
        };
    }

    public enum ShellIconSize { Small, Medium, Large, ExtraLarge, Jumbo }
}