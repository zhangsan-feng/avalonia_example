using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using example.Component.Messages.CreateGroupChatWindow;
using example.Component.Messages.SearchWindow;
using example.Component.Screenshot;
using example.State;
using example.ViewModels;
using ReactiveUI;

namespace example.Component.Messages;

public partial class MessagesView : ReactiveUserControl<MessagesViewModel>{
    public AppStateService State => AppState.Global;

    public MessagesView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });
    }
    
    
    private async void CreateGroupChat(object? sender, RoutedEventArgs e){
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window owner){
            var screenshotWin = new CreateGroupChatWindowView();
            await screenshotWin.ShowDialog(owner);  
        }
     
    }

    private async void SearchGroupAndUser(object? sender, RoutedEventArgs e){
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window owner){
            var screenshotWin = new SearchWindowView();
            await screenshotWin.ShowDialog(owner);  
        }
    }

    private void CopyGroupId(object? sender, RoutedEventArgs e){
        if (sender is MenuItem menuItem) {
            Logger.Log($"点击了群，ID = {menuItem.Tag}");
            var topLevel = TopLevel.GetTopLevel(menuItem);
            if (topLevel?.Clipboard != null){
                _ = topLevel.Clipboard.SetTextAsync(menuItem.Tag.ToString());
                NotificationService.Instance.Show("copy success", "", NotificationType.Success);
            }
        }
    }
}
