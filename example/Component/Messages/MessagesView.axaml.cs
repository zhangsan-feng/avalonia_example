
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;

using example.State;
using ReactiveUI;

namespace example.Component.Messages;

public partial class MessagesView:ReactiveUserControl<MessagesViewModel>{
    public AppStateService State => AppState.Global;
    
    public MessagesView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });

    }
    
    private async void SelectImageFiles(object sender, RoutedEventArgs e){}
    
    private async void SelectFiles(object sender, RoutedEventArgs e){

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            AllowMultiple  = true
        });

        if (files.Count > 0){
            foreach (var file in files){
                Console.WriteLine($"Selected file: {file.Path.AbsolutePath}");
                if (DataContext is MessagesViewModel  vm){
                    vm.SelectedFiles = new ObservableCollection<IStorageFile>(files);
                }
                // 如果你需要 Stream（比如传给 Refit 的 StreamPart）：
                // using var stream = await file.OpenReadAsync();
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

}

