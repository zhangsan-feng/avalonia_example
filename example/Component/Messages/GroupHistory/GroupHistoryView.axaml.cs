using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace example.Component.Messages.GroupHistory;

public partial class GroupHistoryView : ReactiveUserControl<GroupHistoryViewModel>{


    private DateTime _lastClick = DateTime.MinValue;

    private async void Image_PointerPressed(object sender, PointerPressedEventArgs e){
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed){
            var now = DateTime.Now;
            if ((now - _lastClick).TotalMilliseconds < 300){
                var imageUrl = (sender as Border)?.DataContext as string;
                Console.WriteLine(imageUrl);
                if (!string.IsNullOrEmpty(imageUrl) && Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == "http" || uri.Scheme == "https")){
                    await OpenHttpImageInSystemViewer(uri);
                }
            }

            _lastClick = now;
        }
    }

    private async Task OpenHttpImageInSystemViewer(Uri imageUri){
        try{
            using var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(imageUri);

            string ext = Path.GetExtension(imageUri.LocalPath).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !ext.StartsWith("."))
                ext = "."; // 默认
            string tempFile = Path.Combine(Path.GetTempPath(), $"avalonia_img_{Guid.NewGuid()}{ext}");
            await File.WriteAllBytesAsync(tempFile, bytes);

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

            _ = Task.Delay(60 * 1000).ContinueWith(_ => {
                try{
                    File.Delete(tempFile);
                }
                catch{
                    /* ignore */
                }
            });
        }
        catch (Exception ex){
            Console.WriteLine($"Failed to open image: {ex.Message}");
        }
    }

    public GroupHistoryView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });
    }
}