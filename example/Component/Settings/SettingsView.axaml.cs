using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

using example.State;
using ReactiveUI;

namespace example.Component.Settings;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>{
    public AppStateService State => AppState.Global;

    
    public SettingsView(){
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

    private KeyGesture? _capturedGesture; 

    
    private void HotkeyCaptureArea_KeyDown(object? sender, KeyEventArgs e){
        e.Handled = true; // 阻止默认行为（如系统快捷键）

        var display = this.FindControl<TextBlock>("HotkeyDisplay");
        if (display == null) return;

        // 按 Esc 清空
        if (e.Key == Key.Escape){
            ClearCapture(display);
            return;
        }

        bool isModifier = e.Key is Key.LeftCtrl or Key.RightCtrl or
            Key.LeftAlt or Key.RightAlt or
            Key.LeftShift or Key.RightShift or
            Key.LWin or Key.RWin;

        if (isModifier) return;

        // 获取当前修饰键
        var modifiers = e.KeyModifiers;

        // 构建显示文本
        string text = "";
        if ((modifiers & KeyModifiers.Control) != 0) text += "Ctrl + ";
        if ((modifiers & KeyModifiers.Alt) != 0) text += "Alt + ";
        if ((modifiers & KeyModifiers.Shift) != 0) text += "Shift + ";
        if ((modifiers & KeyModifiers.Meta) != 0) text += "Win + ";

        text += e.Key.ToString();
        display.Text = text;
        State.UserName = text;
        _capturedGesture = new KeyGesture(e.Key, modifiers);
    }

    private void ClearCapture(TextBlock display){
        display.Text = "点击此处并按键...";
        _capturedGesture = null;
    }
    
}