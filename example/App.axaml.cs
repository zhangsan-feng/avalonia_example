using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using example.Component.Login;
using example.Component.Messages;
using example.Component.Settings;
using example.Component.Users;
using example.ViewModels;
using example.Views;
using ReactiveUI;
using Splat;
using Tmds.DBus.Protocol;

namespace example;

public partial class App : Application{
    public override void Initialize(){
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted(){
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop){
            
            var loginWindow = new LoginView();
            desktop.MainWindow = loginWindow;
            var loginVm = new LoginViewModel();
            loginWindow.DataContext = loginVm;
            
            loginVm.OnLoginSuccess += () => {
                var mainWindow = new MainWindow {
                    DataContext = new MainWindowViewModel(),
                };
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                loginWindow.Close();
            };
        }

        Locator.CurrentMutable.Register<IViewFor<SettingsViewModel>>(() => new SettingsView());
        Locator.CurrentMutable.Register<IViewFor<MessagesViewModel>>(() => new MessagesView());
        Locator.CurrentMutable.Register<IViewFor<UsersViewModel>>(() => new UsersView());
        
        base.OnFrameworkInitializationCompleted();
    }
}