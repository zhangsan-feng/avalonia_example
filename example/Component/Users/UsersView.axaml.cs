using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace example.Component.Users;

public partial class UsersView:ReactiveUserControl<UsersViewModel>{
    public UsersView(){
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}