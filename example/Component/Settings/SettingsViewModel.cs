using System;
using System.Reactive;
using example.State;
using example.ViewModels;
using ReactiveUI;

namespace example.Component.Settings;

public class SettingsViewModel : ViewModelBase, IRoutableViewModel{
    public AppStateService State => AppState.Global;
    
    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = "SettingsViewModel";

    private string? _userInput;
    public string? UserInput{
        get => _userInput;
        set =>this.RaiseAndSetIfChanged(ref _userInput, value);
    }
    
    public ReactiveCommand<Unit, Unit> SubmitMessageEvent  { get; }
    
    public SettingsViewModel(IScreen screen){
        HostScreen = screen;
        SubmitMessageEvent = ReactiveCommand.Create(() => {

            Console.WriteLine($"Before clear: '{UserInput}'");
            UserInput = "";
            Console.WriteLine($"After clear: '{UserInput}'");
        });
    }
}