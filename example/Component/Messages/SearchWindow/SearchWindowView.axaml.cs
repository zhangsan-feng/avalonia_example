using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using example.State;
using ReactiveUI;

namespace example.Component.Messages.SearchWindow;

public partial class SearchWindowView : ReactiveWindow<SearchWindowViewModel>{
    public SearchWindowView(){
       AvaloniaXamlLoader.Load(this);
       this.WhenActivated(app => { });
       DataContext = new SearchWindowViewModel();
    }


    private async void JoinGroupChatButton_Click(object? sender, RoutedEventArgs e){
        var btn = sender as Button;
        var data = btn.DataContext as SearchResultGroupUi;
        Logger.Log(data.Id);
        if (DataContext is SearchWindowViewModel vm){
            vm.SearchCommandParams = data;
            await vm.JoinGroupChatCommand.Execute();
        }
        
    }
    private async void AddUserFriendButton_Click(object? sender, RoutedEventArgs e){
        var btn = sender as Button;
        var data = btn.DataContext as SearchResultUserUi;
        Logger.Log(data.Id);
    }
}