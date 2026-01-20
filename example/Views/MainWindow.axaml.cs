
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using example.Component.Login;
using example.Component.Screenshot;
using example.State;
using example.ViewModels;
using ReactiveUI;

namespace example.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>{
    public AppStateService State => AppState.Global;
    
    private readonly Subject<string> _messageSubject = new();
    public IObservable<string> Messages => _messageSubject.AsObservable();
    
    public MainWindow(){
        this.WhenActivated(disposables => {
        
        });
        AvaloniaXamlLoader.Load(this);
        NotificationService.Instance.Initialize(this);
    }
    
    private void Mask_PointerPressed(object? sender, PointerPressedEventArgs e){
        if (DataContext is MainWindowViewModel vm)
            vm.BorderIsVisible = false;
    }
    
    protected override async void OnDataContextChanged(EventArgs e){
        await State.InitWsAsync();
    }

    protected override void OnClosed(EventArgs e){
        base.OnClosed(e);
        State.Dispose();
    }
}

