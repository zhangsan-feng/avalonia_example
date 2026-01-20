using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace example.Component.Login;

public partial class LoginView : ReactiveWindow<LoginViewModel>{
    public LoginView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });
        DataContext =  new LoginViewModel();
    }
    
    // private async void PickAvatarCommand(object? sender, RoutedEventArgs e){
    //     var topLevel = TopLevel.GetTopLevel(this);
    //     if (topLevel == null) return;
    //     var imageTypes = new FilePickerFileType("Image files") {
    //         Patterns = ["*.jpg", "*.jpeg", "*.png", "*.webp", "*.bmp"],
    //         MimeTypes = ["image/jpeg", "image/png", "image/webp", "image/bmp"]
    //     };
    //     var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
    //         FileTypeFilter = new[] { imageTypes, }
    //     });
    //
    //     if (files.Count != 0) {
    //         if (DataContext is LoginViewModel vm){
    //             var file = files[0];
    //             var properties = await file.GetBasicPropertiesAsync();
    //             if (properties.Size > 8 * 1024 * 1024){
    //                 vm.ErrorMessage = "图片字节太大";
    //             }
    //             vm.SelectedAvatar = file;
    //             using var stream = await file.OpenReadAsync();
    //             var bitmap = await Task.Run(() => new Bitmap(stream));
    //             vm.SelectedAvatarUi = bitmap;
    //         }
    //     }
    // }
}