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

public static class ImageHelper{
    private static readonly HttpClient _httpClient = new(); 
    private static readonly ConcurrentDictionary<string, Task<Bitmap?>> _cache = new();
    private static readonly SemaphoreSlim _decodeSemaphore = new(4, 4); 

    public static async Task<Bitmap?> LoadFromWeb(Uri url){
        var key = url.ToString();
        return await _cache.GetOrAdd(key, async (k) => {
            try{
                var response = await _httpClient.GetAsync(new Uri(k));
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync();

                await _decodeSemaphore.WaitAsync();
                try{
                    return await Task.Run(() => {
                        try{
                            return new Bitmap(new MemoryStream(data));
                        }
                        catch{
                            return null;
                        }
                    });
                }
                finally{
                    _decodeSemaphore.Release();
                }
            }
            catch (Exception ex){
                Console.WriteLine($"Failed to load image from {k}: {ex.Message}");
                return null;
            }
        });
    }

    public static Bitmap LoadFromResource(Uri resourceUri){
        return new Bitmap(AssetLoader.Open(resourceUri));
    }
}


public class GroupMembersUi{
    public string? Id{ get; set; }
    public string? Name{ get; set; }
    public string? UserType{ get; set; }
    public Bitmap? Avatar{ get; set; }
    public string? Status {get;set;}

    public override string ToString(){
        return $"Id:{Id}, Name:{Name}, UserType:{UserType}, Status:{Status}";
    }
    
    public static async Task<GroupMembersUi> FromPayloadAsync(GroupMembersHttp payload){
        if (payload == null)
            return null;
        return new GroupMembersUi {
            Id = payload.Id,
            Name = payload.Name,
            Avatar = await ImageHelper.LoadFromWeb(new Uri(payload.Avatar)),
            UserType = payload.Usertype != null ? payload.Usertype : "普通群员",
            Status = payload.Status
        };
    }
}


public class GroupHistoryMessageUi : ReactiveObject{
    public AppStateService State => AppState.Global;
    public string? MessageId{ get; set; }
    public string? SendGroupId{ get; set; }
    public string? SendUserId{ get; set; }
    public string? SendUserName{ get; set; }
    public Bitmap? SendUserAvatar{ get; set; }
    public string? Message{ get; set; }
    public string? Time{ get; set; }
    public string?[] Files{ get; set; }
    public bool IsShowMessage{ get; set; }
    
    public bool IsSelf => SendUserId == State.UserId;
    
    public static async Task<GroupHistoryMessageUi> FromPayloadAsync(GroupHistoryMessageHttp payload){
        if (payload == null)
            return null;

        return new GroupHistoryMessageUi {
            SendUserId = payload.SendUserId,
            SendUserName = payload.SendUserName,
            SendUserAvatar = await ImageHelper.LoadFromWeb(new Uri(payload.SendUserAvatar)),
            MessageId = payload.MessageId,
            SendGroupId = payload.SendGroupId,
            Message = payload.Message,
            Time = payload.Time,
            Files = payload.Files,
            IsShowMessage = payload.Files.Length > 0 ? false : true,
        };
    }
}



public class UserMessageGroupUi : ReactiveObject{
    public string? Id{ get; set; }
    public string? Name{ get; set; }
    public Bitmap? Avatar{ get; set; }
    public int MessageNumber{ get; set; }
    public GroupHistoryMessageHttp[] History{ get; set; }
    public ObservableCollection<GroupHistoryMessageUi> HistoryCache { get; set; } = new();
    public bool IsHistoryLoaded { get; set; } = false;
    public bool IsShowGroupMember { get; set; } = false;
    public string? GroupType{ get; set; }
    private ObservableCollection<GroupMembersUi> _members = new();
    public ObservableCollection<GroupMembersUi> Members{
        get => _members;
        set => this.RaiseAndSetIfChanged(ref _members, value);
    }
    
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
        Avatar = await ImageHelper.LoadFromWeb(new Uri(data.Avatar));
        History = data.History ?? Array.Empty<GroupHistoryMessageHttp>();
        MessageNumber = History.Length;
        LastMessage = History.Length > 0 ? History[^1] : new GroupHistoryMessageHttp();
        ShowMessageNumber = MessageNumber > 0;
        
        if (data.Type == "private_chat"){
            IsShowGroupMember = false;
            GroupType = "private_chat";
        }else{
            // group_chat
            IsShowGroupMember = true;
            GroupType = "group_member";
            
        }

        if (data.Members != null && data.Members.Length > 0){
            
            var memberTasks = data.Members.Select(GroupMembersUi.FromPayloadAsync);
            Members =  new ObservableCollection<GroupMembersUi>(await Task.WhenAll(memberTasks));
        }
        else{
            Members =  new ObservableCollection<GroupMembersUi>();
            
        }
    }
}


public class SearchResultGroupUi{
    public string? Id{ get; set; }
    public string? Name{ get; set; }
    public Bitmap? Avatar{ get; set; }
    public string? Type{ get; set; }
    public ObservableCollection<GroupMembersUi> Members{ get; set; } = new();

    public static async Task<SearchResultGroupUi> FromPayloadAsync(SearchResultGroupHttp payload){
        if (payload == null)
            return null;

        var ui = new SearchResultGroupUi{
            Id = payload.Id,
            Name = payload.Name,
            Type = payload.Type,
            Avatar = await ImageHelper.LoadFromWeb(new Uri(payload.Avatar)),
        };

        if (payload.Members != null && payload.Members.Length > 0){
            var memberTasks = payload.Members.Select(GroupMembersUi.FromPayloadAsync);
            ui.Members = new ObservableCollection<GroupMembersUi>(await Task.WhenAll(memberTasks));
        }

        return ui;
    }
}


public class SearchResultUserUi{
    public string? Id{ get; set; }
    public string? Name{ get; set; }
    public Bitmap? Avatar{ get; set; }
    public string? Status{ get; set; }

    public static async Task<SearchResultUserUi> FromPayloadAsync(SearchResultUserHttp payload){
        if (payload == null)
            return null;

        return new SearchResultUserUi{
            Id = payload.Id,
            Name = payload.Name,
            Avatar = await ImageHelper.LoadFromWeb(new Uri(payload.Avatar)),
            Status = payload.Status,
        };
    }
}
