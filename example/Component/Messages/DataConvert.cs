using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using example.State;
using ReactiveUI;

namespace example.Component.Messages;


public class GroupMembersUi{
    public string Name{ get; set; }
    public string Id{ get; set; }
    public string UserTyoe{ get; set; }
    public string Avatar{ get; set; }

    public static async Task<GroupMembersUi> FromPayloadAsync(GroupMembersHttp payload){
        if (payload == null)
            return null;
        

        return new GroupMembersUi {
            Id = payload.Id,
            Name = payload.Name,
            Avatar = payload.Avatar,
            UserTyoe = "普通群员"
        };
    }
}


public class GroupHistoryMessageUi : ReactiveObject{
    public AppStateService State => AppState.Global;
    public string MessageId{ get; set; }
    public string SendGroupId{ get; set; }
    public string SendUserId{ get; set; }
    public string SendUserName{ get; set; }
    public string SendUserAvatar{ get; set; }
    public string Message{ get; set; }
    public string Time{ get; set; }
    public string?[] Files{ get; set; }
    public bool IsSelf => SendUserId == State.UserId;
    
    public static async Task<GroupHistoryMessageUi> FromPayloadAsync(GroupHistoryMessageHttp payload){
        if (payload == null)
            return null;

        return new GroupHistoryMessageUi {
            SendUserId = payload.SendUserId,
            SendUserName = payload.SendUserName,
            SendUserAvatar = payload.SendUserAvatar,
            MessageId = payload.MessageId,
            SendGroupId = payload.SendGroupId,
            Message = payload.Message,
            Time = payload.Time,
            Files = payload.Files
        };
    }
}



public class UserMessageGroupUi : ReactiveObject{
    public string Id{ get; set; }
    public string Name{ get; set; }
    public string? Avatar{ get; set; }
    public int MessageNumber{ get; set; }
    public GroupHistoryMessageHttp[] History{ get; set; }
    public GroupMembersUi[] Members{ get; set; }
    public ObservableCollection<GroupHistoryMessageUi> HistoryCache { get; set; } = new();
    public bool IsHistoryLoaded { get; set; } = false;
    
    private bool _showMessageNumber = false;
    public bool ShowMessageNumber{
        get => _showMessageNumber;
        set => this.RaiseAndSetIfChanged(ref _showMessageNumber, value);
    }
    
    private GroupHistoryMessageHttp _lastMessage;
    public GroupHistoryMessageHttp LastMessage{
        get => _lastMessage;
        set => this.RaiseAndSetIfChanged(ref _lastMessage, value);
    }
    
    public static async Task<UserMessageGroupUi> FromPayloadAsync(UserMessageGroupHttp data){
        var instance = new UserMessageGroupUi();
        await instance.ConvertFromPayloadAsync(data);
        return instance;
    }

    private async Task ConvertFromPayloadAsync(UserMessageGroupHttp data){
        Id = data.Id;
        Name = data.Name;
        Avatar = data.Avatar;
        History = data.History ?? Array.Empty<GroupHistoryMessageHttp>();
        MessageNumber = History.Length;
        LastMessage = History.Length > 0 ? History[^1] : new GroupHistoryMessageHttp();
        ShowMessageNumber = MessageNumber > 0;
        if (data.Members.Length > 0){
            var memberTasks = data.Members.Select(GroupMembersUi.FromPayloadAsync);
            Members = await Task.WhenAll(memberTasks);
        }
        else{
            Members = Array.Empty<GroupMembersUi>();
        }
    }
}

