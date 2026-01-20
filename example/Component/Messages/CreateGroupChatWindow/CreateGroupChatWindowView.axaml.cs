using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using example.State;
using ReactiveUI;

namespace example.Component.Messages.CreateGroupChatWindow;

public partial class CreateGroupChatWindowView : ReactiveWindow<CreateGroupChatWindowViewModel>{
    public CreateGroupChatWindowView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });
        DataContext = new CreateGroupChatWindowViewModel();
    }

    private async void PickAvatarCommand(object? sender, RoutedEventArgs e){
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        var imageTypes = new FilePickerFileType("Image files") {
            Patterns = ["*.jpg", "*.jpeg", "*.png", "*.webp", "*.bmp"],
            MimeTypes = ["image/jpeg", "image/png", "image/webp", "image/bmp"]
        };
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            FileTypeFilter = new[] { imageTypes, }
        });
        Logger.Log(files.Count);
        if (files.Count != 0) {
 
            if (DataContext is CreateGroupChatWindowViewModel vm){
                var file = files[0];
                var properties = await file.GetBasicPropertiesAsync();
                if (properties.Size > 8 * 1024 * 1024){
                    return;
                }
                vm.SelectedAvatar = file;
                vm.HasSelectedAvatar = false;
                var stream = await file.OpenReadAsync();
                var bitmap = await Task.Run(() => new Bitmap(stream));
                vm.SelectedAvatarUi = bitmap;
            }
        }
    }

    private async void CreateGroupCommand(object? sender, RoutedEventArgs e){
        if (DataContext is CreateGroupChatWindowViewModel vm){
            await vm.CreateGroupCommand.Execute().ToTask();
            this.Close();
        }
    }
}