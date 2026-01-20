using System;
using System.Reactive.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using example.State;
using ReactiveUI;

namespace example.Component.Messages.ChatBox;

public partial class ChatBoxView : ReactiveUserControl<ChatBoxViewModel>{
    
    private async void SelectFiles(object sender, RoutedEventArgs e){
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            AllowMultiple = true,
            // FileTypeFilter = new[] { imageTypes, videoTypes }
        });
        if (files.Count > 0){
            if (DataContext is ChatBoxViewModel vm){
                vm.SelectedFiles = files;
                await vm.SubmitMessageEvent.Execute().ToTask();
            }

            foreach (var file in files){
                Logger.Log($"Selected file: {file.Path.AbsolutePath}");
            }
        }
    }
    
    public ChatBoxView(){
        
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => {

        }); 
    }
}