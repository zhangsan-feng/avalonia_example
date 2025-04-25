using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using desktop_application.ViewModels;
using desktop_application.Views;
using desktop_application.Views.Home;
using desktop_application.Views.Settings;
using ReactiveUI;
using Splat;


namespace desktop_application;

public partial class App : Application{
    public App (){
        
    }

    public override void Initialize(){
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted(){
        Locator.CurrentMutable.Register(() => new HomeView(), typeof(IViewFor<HomeViewModel>));
        Locator.CurrentMutable.Register(() => new SettingsView(), typeof(IViewFor<SettingsViewModel>));
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop){
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
                Width = 1000,  
                Height = 500, 
                CanResize = false ,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
       
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}