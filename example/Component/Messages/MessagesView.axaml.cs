using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using example.State;
using ReactiveUI;

namespace example.Component.Messages;

public partial class MessagesView : ReactiveUserControl<MessagesViewModel>{
    public AppStateService State => AppState.Global;

    public MessagesView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });
    }
    
  
    
}