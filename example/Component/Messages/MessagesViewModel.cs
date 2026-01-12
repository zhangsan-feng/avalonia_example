using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DynamicData;
using example.Component.Messages.ChatBox;
using example.Component.Messages.GroupHistory;
using example.Component.Messages.GroupMembers;
using example.State;
using example.ViewModels;
using ReactiveUI;
using Refit;

namespace example.Component.Messages;

public class MessagesViewModel : ViewModelBase, IRoutableViewModel{
    public IScreen HostScreen{ get; }
    public string UrlPathSegment{ get; } = "MessagesViewModel";


    public AppStateService State => AppState.Global;
    public readonly ChatMessageApiInterface ChatMessageApi;


    public ChatBoxViewModel ChatBoxVm{ get; set; }
    public GroupHistoryViewModel GroupHistoryVm{ get; set; }
    public GroupMembersViewModel GroupMemberVm{ get; set; }


    private ObservableCollection<UserMessageGroupUi> _messageGroup;

    public ObservableCollection<UserMessageGroupUi> MessageGroup{
        get => _messageGroup;
        set => this.RaiseAndSetIfChanged(ref _messageGroup, value);
    }


    private UserMessageGroupUi? _selectMessageGroupIndex;
    public UserMessageGroupUi? SelectMessageGroupIndex{
        get => _selectMessageGroupIndex;
        set{
            this.RaiseAndSetIfChanged(ref _selectMessageGroupIndex, value);
            ChatBoxVm.SelectGroupId = value.Id;
        }
    }

    private bool messageWindowIsShow = true;

    public bool MessageWindowIsShow{
        get => messageWindowIsShow;
        set => this.RaiseAndSetIfChanged(ref messageWindowIsShow, value);
    }

    
    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Clear();

    
    private async Task InitializeAsync(){
        try{
            var res = await ChatMessageApi.GetMessageGroup(State.UserId);
            // Console.WriteLine(res.Data);
            var tasks = res.Data.Select(UserMessageGroupUi.FromPayloadAsync);
            MessageGroup = new ObservableCollection<UserMessageGroupUi>(await Task.WhenAll(tasks));
            MessageWindowIsShow = true;
            if (MessageGroup.Count > 0){
                SelectMessageGroupIndex = MessageGroup[0];
                GroupMemberVm.GroupMembers = new ObservableCollection<GroupMembersUi>(
                    SelectMessageGroupIndex.Members ?? Enumerable.Empty<GroupMembersUi>()
                );
            }
        }
        catch (Exception ex){
            Console.WriteLine($"API Error: {ex}");
        }
    }


    public MessagesViewModel(IScreen screen){
        HostScreen = screen;
        ChatMessageApi = RestService.For<ChatMessageApiInterface>("http" + State.ServerAddress);
        
        _ = InitializeAsync();

        ChatBoxVm = new ChatBoxViewModel();
        GroupHistoryVm = new GroupHistoryViewModel();
        GroupMemberVm = new GroupMembersViewModel();
        
        var parsedMessages = AppState.Global.Messages
            .Select(msg => {
                try{
                    return JsonSerializer.Deserialize<WebSocketMessage>(msg);
                }
                catch (Exception ex){
                    Console.WriteLine($"Parse error: {ex.Message}");
                    return null;
                }
            })
            .Where(header => header != null)
            .Publish()
            .RefCount();

        parsedMessages
            .Where(data => data.Type == "message")
            .Select(msg => JsonSerializer.Deserialize<GroupHistoryMessageHttp>(msg.Data))
            .Where(payload => payload != null)
            .SelectMany(async payload => new {
                Payload = payload,
                UI = await GroupHistoryMessageUi.FromPayloadAsync(payload)
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(result => {
                // Console.WriteLine(result);
                var targetGroup = MessageGroup.FirstOrDefault(g => g.Id == result.Payload.SendGroupId);
                if (targetGroup != null){
                    targetGroup.HistoryCache.Add(result.UI);
                    targetGroup.LastMessage = result.Payload;

                    Console.WriteLine($"recv group: [ {targetGroup.Id} ] message");
                }
            })
            .DisposeWith(_disposables);

        parsedMessages
            .Where(data => data.Type == "join_group")
            .SelectMany(async msg => {
                try{
                    var payload = msg.Data.Deserialize<GroupMembersHttp>();
                    if (payload == null) return null;

                    var ui = await GroupMembersUi.FromPayloadAsync(payload);
                    return new {
                        GroupId = msg.GroupId,
                        Payload = payload,
                        UI = ui
                    };
                }
                catch (Exception ex){
                    Console.WriteLine($"Error processing join_group: {ex}");
                    return null;
                }
            })
            .Where(result => result != null) // 过滤掉失败的
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(result => {
                Console.WriteLine($"Processing join_group for group: {result.GroupId}");
                
                var targetGroup = MessageGroup.FirstOrDefault(g => g.Id == result.GroupId);
                if (targetGroup != null){
                    Console.WriteLine(result.UI.GetType());
                    targetGroup.Members.Add(result.UI);
                    Console.WriteLine($"Updated group: [ {targetGroup.Id} ]");
                }
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectMessageGroupIndex)
            .WhereNotNull()
            .Do(item => item.ShowMessageNumber = false)
            .SelectMany(async groupIndex => {
                if (!groupIndex.IsHistoryLoaded) {
                    var results = await Task.WhenAll(groupIndex.History.Select(GroupHistoryMessageUi.FromPayloadAsync));
                    return (groupIndex, results);
                }
                return (groupIndex, (GroupHistoryMessageUi[])null);
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(data => {
                var (groupIndex, results) = data;
                if (results != null) {
                    groupIndex.HistoryCache.Clear();
                    foreach (var r in results) groupIndex.HistoryCache.Add(r);
                    groupIndex.IsHistoryLoaded = true;
                }
                GroupHistoryVm.GroupHistoryMessage = groupIndex.HistoryCache;
                GroupMemberVm.GroupMembers = groupIndex.Members;
            }, ex => Console.WriteLine($"Error loading history: {ex}"))
            .DisposeWith(_disposables);
    }
}