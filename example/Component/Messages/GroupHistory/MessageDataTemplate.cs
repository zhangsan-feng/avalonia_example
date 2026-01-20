using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using example.State;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace example.Component.Messages.GroupHistory;

// FileTemplateSelector.cs
using Avalonia.Controls;
using Avalonia.Controls.Templates;

public class MessageDataTemplate : IDataTemplate{
    private string fileName;
    private static readonly LibVLC _libVLC = new();

    public Control Build(object? param){
        if (param is not string filePath)
            return new Border { Width = 50, Height = 50 };

        var grid = new Grid {
            Width = 50,
            Height = 50,
            Margin = new Thickness(10, 15, 10, 0),
        };
        var overlay = new Border {
            Background = Brushes.Transparent,
            ZIndex = 111
        };
        
        if (IsImage(filePath)){
            var image = new Image();
            ImageLoader.SetSource(image, filePath);
            grid.Children.Add(image);
        }
        else if (IsVideo(filePath)){
            overlay.Background = Brushes.DarkGray;
            grid.Children.Add(new TextBlock(){Text = param.ToString(),TextTrimming=TextTrimming.CharacterEllipsis}); 
        }
        
        
        overlay.DoubleTapped += async (sender, e) => {
            e.Handled = true;
            if (Uri.TryCreate(filePath, UriKind.Absolute, out var uri) &&
                (uri.Scheme == "http" || uri.Scheme == "https")){
                await OpenHttpImageInSystemViewer(uri);
            }
        };
        
        grid.Children.Add(overlay); 
        return grid;
    }

    private async Task OpenHttpImageInSystemViewer(Uri imageUri){
        try{
            using var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(imageUri);

            string ext = Path.GetExtension(imageUri.LocalPath).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !ext.StartsWith(".")){
                ext = ".";
            }

            string tempFile = Path.Combine(Path.GetTempPath(), $"avalonia_img_{Guid.NewGuid()}{ext}");
            if (!File.Exists(tempFile)){
                await File.WriteAllBytesAsync(tempFile, bytes);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                Process.Start(new ProcessStartInfo {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){
                Process.Start("open", tempFile);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                Process.Start("xdg-open", tempFile);
            }

            // _ = Task.Delay(60 * 1000).ContinueWith(_ => {
            //     try{
            //         File.Delete(tempFile);
            //     }
            //     catch{
            //         /* ignore */
            //     }
            // });
        }
        catch (Exception ex){
            Logger.Log($"Failed to open image: {ex.Message}");
        }
    }

    private static bool IsImage(string path){
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp";
    }

    private static bool IsVideo(string path){
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".mp4" or ".mkv" or ".avi" or ".mov" or ".wmv";
    }

    public bool Match(object? data){
        return data != null;
    }
}