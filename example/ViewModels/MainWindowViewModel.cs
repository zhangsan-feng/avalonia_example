using System;
using System.Collections.Generic;
using System.Reactive;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using example.Component.Messages;
using ReactiveUI;
using example.Component.Settings;
using example.Component.Users;
using example.State;


namespace example.ViewModels;


public class MenuItemViewModel(string title,string icon, Type viewModelType) : ViewModelBase{
    public string Title { get; } = title;
    public Type ViewModelType { get; } = viewModelType;
    public Bitmap Icon { get; } = new Bitmap(AssetLoader.Open(new Uri(icon)));
}

public class MainWindowViewModel : ViewModelBase, IScreen {
    private readonly Dictionary<Type, IRoutableViewModel> _viewModelCache = new();
    
    public List<MenuItemViewModel> MenuItems{ get; } = [
        new("", "avares://example/Assets/message_icon.png", typeof(MessagesViewModel)),
        new("", "avares://example/Assets/user_icon.png", typeof(UsersViewModel)),
        new("", "avares://example/Assets/settings_icon.png", typeof(SettingsViewModel)),
    ];
    public RoutingState Router { get; } = new RoutingState();
    
    private MenuItemViewModel? _selectedMenuItem;
    public MenuItemViewModel? SelectedMenuItem{
        get => _selectedMenuItem;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
    }
    
    bool _borderIsVisible;
    public bool BorderIsVisible{
        get => _borderIsVisible;
        set => this.RaiseAndSetIfChanged(ref _borderIsVisible, value);
    }

    public ReactiveCommand<Unit, Unit> Button_OnClick { get; }
    
    public AppStateService State => AppState.Global;
    public MainWindowViewModel(){
        LibVLCSharp.Shared.Core.Initialize();
        
        SelectedMenuItem = MenuItems[0];
        
        Button_OnClick = ReactiveCommand.Create(() => { BorderIsVisible = !BorderIsVisible; });
        
        this.WhenAnyValue(x => x.SelectedMenuItem)
            .WhereNotNull()
            .Subscribe(item => {
                var vmType = item.ViewModelType;

                if (!_viewModelCache.TryGetValue(vmType, out var vm)){
                    vm = (IRoutableViewModel)Activator.CreateInstance(vmType, this)!;
                    _viewModelCache[vmType] = vm;
                }
                Router.Navigate.Execute(vm);
            });
    }
    
}