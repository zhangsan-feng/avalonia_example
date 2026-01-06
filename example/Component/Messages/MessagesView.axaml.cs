using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using example.State;
using ReactiveUI;

namespace example.Component.Messages;

public partial class MessagesView : ReactiveUserControl<MessagesViewModel>{
    public AppStateService State => AppState.Global;

    public MessagesView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });
    }
    
    private async void SelectFiles(object sender, RoutedEventArgs e){
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        var imageTypes = new FilePickerFileType("Image files") {
            Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp", "*.webp" },
            MimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" }
        };

        var videoTypes = new FilePickerFileType("Video files") {
            Patterns = new[] { "*.mp4",  },
            MimeTypes = new[] { "video/mp4",  }
        };
        var audioTypes = new FilePickerFileType("Audio files") {
            Patterns = ["*.mp3","*.wav"],
            MimeTypes = ["audio/mpeg", "audio/wav",  "audio/mp3", ]
        };
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            AllowMultiple = true,
            FileTypeFilter = new[] { imageTypes, videoTypes, audioTypes }
        });

        if (files.Count > 0){
            if (DataContext is MessagesViewModel vm){
                vm.SelectedFiles = files;
                vm.SubmitMessageEvent.Execute().Subscribe(ex => { });
            }

            foreach (var file in files){
                Console.WriteLine($"Selected file: {file.Path.AbsolutePath}");
            }
        }
    }

    private async void InputBox_KeyDown(object? sender, KeyEventArgs e){
        e.Handled = true;

        if (e.Key == Key.B && e.KeyModifiers.HasFlag(KeyModifiers.Control)){
            var topLevel = TopLevel.GetTopLevel(this);
            var clipboard = topLevel?.Clipboard;
            if (clipboard == null)
                return;

            var formats = await clipboard.TryGetBitmapAsync();
            Console.WriteLine($"图片尺寸: {formats.Size.Width} x {formats.Size.Height}");
            if (formats != null){
                (DataContext as MessagesViewModel)?.AddImage(formats);
            }
        }
    }

    private DateTime _lastClick = DateTime.MinValue;

    private async void Image_PointerPressed(object sender, PointerPressedEventArgs e){
        Console.WriteLine("Image_PointerPressed event fired.");
        
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed){
            Console.WriteLine(1111111111111);
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
                ext = ".png"; // 默认
            string tempFile = Path.Combine(Path.GetTempPath(), $"avalonia_img_{Guid.NewGuid()}{ext}");
            await File.WriteAllBytesAsync(tempFile, bytes);
            Console.WriteLine(tempFile);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                Process.Start(new ProcessStartInfo {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            _ = Task.Delay(60_000).ContinueWith(_ => {
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
}