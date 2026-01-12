using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace example.Component.Messages.ChatBox;

public partial class ChatBoxView : ReactiveUserControl<ChatBoxViewModel>{
    
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
            if (DataContext is ChatBoxViewModel vm){
                vm.SelectedFiles = files;
                vm.SubmitMessageEvent.Execute().Subscribe(ex => { });
            }

            foreach (var file in files){
                Console.WriteLine($"Selected file: {file.Path.AbsolutePath}");
            }
        }
    }
    
    public ChatBoxView(){
        
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => {

        }); 
    }
}