using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;

namespace example.ViewModels;

public interface INotificationService{
    void Show(string title, string message, NotificationType type = NotificationType.Information);
}

public class NotificationService {
 
    public static NotificationService Instance { get; } = new NotificationService();
    private WindowNotificationManager? _manager;
    private NotificationService() { }
    
    public void Initialize(Window hostWindow){
        _manager = new WindowNotificationManager(hostWindow) {
            Position = NotificationPosition.TopCenter,
            MaxItems = 3,
            Margin = new Avalonia.Thickness(0, 10, 10, 0)
        };
    }
    
    public void Show(string title, string message, NotificationType type = NotificationType.Information){
        if (_manager == null){
            System.Diagnostics.Debug.WriteLine("通知管理器未初始化！");
            return;
        }
        
        Dispatcher.UIThread.InvokeAsync(() => {
            _manager.Show(new Notification(
                title, 
                message, 
                type, 
                TimeSpan.FromSeconds(3)));
        });
    }
}