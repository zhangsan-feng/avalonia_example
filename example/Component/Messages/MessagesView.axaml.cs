
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using AvRichTextBox;
using example.State;
using ReactiveUI;

namespace example.Component.Messages;

public partial class MessagesView:ReactiveUserControl<MessagesViewModel>{
    public AppStateService State => AppState.Global;
    
    public MessagesView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });

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

