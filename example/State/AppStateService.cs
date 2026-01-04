using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

namespace example.State;

public class AppStateService : ReactiveObject{
    private string _userName = "默认用户名";
    private string _userId = "0b5ebfcb-dda4-44a7-bc55-66a9d21cf082";
    private static string _serverAddress = "http://127.0.0.1:34332";
    private string _userAvatar = "";
    private string _userToken = "";

    public string UserToken{
        get => _userToken;
        set => this.RaiseAndSetIfChanged(ref _userToken, value);
    }

    public string UserAvatar{
        get => _userAvatar;
        set => this.RaiseAndSetIfChanged(ref _userAvatar, value);
    }

    public string ServerAddress{
        get => _serverAddress;
        set => this.RaiseAndSetIfChanged(ref _serverAddress, value);
    }

    public string UserName{
        get => _userName;
        set => this.RaiseAndSetIfChanged(ref _userName, value);
    }

    public string UserId{
        get => _userId;
        set => this.RaiseAndSetIfChanged(ref _userId, value);
    }

    private readonly Subject<string> _messages = new();
    public IObservable<string> Messages => _messages.AsObservable();
    public void PushMessage(string msg) => _messages.OnNext(msg);


    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _disposed = false;

    public void Dispose(){
        if (_disposed) return;
        _disposed = true;
        _cancellationTokenSource?.Cancel(); 
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }

    public async Task InitWsAsync(){
        while (!_disposed){
            try{
                if (_webSocket?.State == WebSocketState.Open)
                    return; 
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
                _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20); 
                _webSocket.Options.SetRequestHeader("Origin", "http://localhost:34332");

                var cancellationToken = _cancellationTokenSource?.Token ?? CancellationToken.None;
                await _webSocket.ConnectAsync(new Uri("ws://127.0.0.1:34332/register_ws"), cancellationToken);

                Console.WriteLine("✅ WebSocket 连接成功！");
                _ = ReceiveMessagesAsync(cancellationToken);

                await SendMessageAsync($"{{\"id\":\"{UserId}\", \"token\":\"asdasdas123123\"}}");

                _ = MonitorConnectionAsync(cancellationToken);

                return; 
            }
            catch (Exception ex){
                Console.WriteLine($"❌ 连接失败: {ex.Message}");
                if (_disposed) break;
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }


    private async Task MonitorConnectionAsync(CancellationToken cancellationToken){
        while (!cancellationToken.IsCancellationRequested){
            try{
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                if (_webSocket == null || _webSocket.State != WebSocketState.Open){
                    Console.WriteLine("⚠️ 检测到连接断开，尝试重连...");
                    await InitWsAsync(); 
                    break;
                }
            }catch (Exception ex){
                Console.WriteLine($"Monitor error: {ex}");
                break;
            }
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken){
        if (_webSocket == null) return;

        var buffer = new byte[4096];

        try{
            while (_webSocket.State == WebSocketState.Open &&
                   !cancellationToken.IsCancellationRequested){
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close){
                    // 服务器请求关闭连接
                    await _webSocket.CloseAsync(
                        result.CloseStatus.Value,
                        result.CloseStatusDescription,
                        cancellationToken);
                    Console.WriteLine("连接已关闭");
                    break;
                }

    
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"收到消息: {message}");
                if (message != "ping"){
                    PushMessage(message);
                }
            }
        }
        catch (Exception ex){
            Console.WriteLine($"接收消息出错: {ex.Message}");
        }
    }

    public async Task SendMessageAsync(string message){
        if (_webSocket?.State != WebSocketState.Open){
            Console.WriteLine("WebSocket未连接，无法发送消息");
            return;
        }

        try{
            var buffer = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true, // 表示这是消息的最后一段
                _cancellationTokenSource?.Token ?? CancellationToken.None);

            Console.WriteLine($"已发送: {message}");
        }
        catch (Exception ex){
            Console.WriteLine($"发送消息失败: {ex.Message}");
        }
    }

    
}