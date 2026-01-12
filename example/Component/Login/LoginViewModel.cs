using System;
using System.Reactive;
using example.State;
using example.ViewModels;
using ReactiveUI;
using Refit;


namespace example.Component.Login;

public class LoginViewModel : ViewModelBase{
    public AppStateService State => AppState.Global;
    private string? _userName;
    private string? _password;
    private string? _errorMessage;

    public string? UserName{
        get => _userName;
        set{
            if (value != null && value.Length > 16){
                ErrorMessage = "用户名太长了";
            }
            else{
                ErrorMessage = "";
            }
            this.RaiseAndSetIfChanged(ref _userName, value);
        }
    }

    public string? Password{
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string? ErrorMessage{
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }
    
    public readonly LoginApiInterface LoginApi;  

    public Action? OnLoginSuccess{ get; set; }


    public ReactiveCommand<Unit, Unit> LoginCommand{ get; }

    public LoginViewModel(){
        LoginApi = RestService.For<LoginApiInterface>("http" + State.ServerAddress);
        
        LoginCommand = ReactiveCommand.CreateFromTask(async () => {
            if (UserName != null && UserName.Length > 16){
                return;
            }
            try{
                var result = await LoginApi.Login( new LoginParams{
                    UserName = UserName,
                    Password = Password,
                });
                Console.WriteLine(result.uuid);
                Console.WriteLine(result.token);
                State.UserId = result.uuid;
                State.UserToken = result.token;
                
                OnLoginSuccess?.Invoke();

            }
            catch (Exception e){
                Console.WriteLine(e);
                throw;
            }

            // if (Password == "123"){
                // State.UserId = UserName;
                // OnLoginSuccess?.Invoke();
            // }
            // else{
                // ErrorMessage = "❌ 账号或密码错误，请重试";
            // }
        });
    }
}