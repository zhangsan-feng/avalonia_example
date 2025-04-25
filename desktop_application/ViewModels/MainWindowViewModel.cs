
using System.Reactive;
using ReactiveUI;


namespace desktop_application.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new();

    public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }

    public MainWindowViewModel()
    {
        
        GoHome = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new HomeViewModel(this)));
            
        GoSettings = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new SettingsViewModel(this)));
    
        Router.Navigate.Execute(new HomeViewModel(this));
    }
}