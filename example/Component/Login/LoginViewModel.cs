using System;
using System.Reactive;
using example.State;
using example.ViewModels;
using ReactiveUI;


namespace example.Component.Login;

public class LoginViewModel : ViewModelBase{
    public AppStateService State => AppState.Global;
    private string? _userName;
    private string? _password;
    private string? _errorMessage;

    public string? UserName{
        get => _userName;
        set => this.RaiseAndSetIfChanged(ref _userName, value);
    }

    public string? Password{
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string? ErrorMessage{
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }


    public Action? OnLoginSuccess{ get; set; }


    public ReactiveCommand<Unit, Unit> LoginCommand{ get; }

    public LoginViewModel(){
        LoginCommand = ReactiveCommand.Create(() => {
            if (Password == "123"){
                State.UserId = UserName;
                OnLoginSuccess?.Invoke();
            }
            else{
                ErrorMessage = "❌ 账号或密码错误，请重试";
            }
        });
    }
}