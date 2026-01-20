using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
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
    public readonly ChatMessageApi ChatMessageApi;


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
            // Logger.Log(res.Data);
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
            Logger.Log($"API Error: {ex}");
        }
    }

    private void MoveMessavePostion(UserMessageGroupUi targetGroup){
        if (SelectMessageGroupIndex != targetGroup){
            targetGroup.MessageNumber++;
            targetGroup.ShowMessageNumber = true;
        }
                    
        var oldIndex = MessageGroup.IndexOf(targetGroup);
        if (oldIndex > 0){
            MessageGroup.Move(oldIndex, 0);
            SelectMessageGroupIndex = targetGroup;
        }
    }

    public MessagesViewModel(IScreen screen){
        HostScreen = screen;
        ChatMessageApi = RestService.For<ChatMessageApi>("http" + State.ServerAddress);

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
                    Logger.Log($"Parse error: {ex.Message}");
                    return null;
                }
            })
            .Where(header => header != null)
            .Publish()
            .RefCount();

        parsedMessages
            .Where(data => data.Type == "message")
            .SelectMany(async payload => {
                var p = JsonSerializer.Deserialize<GroupHistoryMessageHttp>(payload.Data);
                return new {
                    Payload = p,
                    UI = await GroupHistoryMessageUi.FromPayloadAsync(p)
                };
            })
            .Where(result => result != null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(result => {

                var targetGroup = MessageGroup.FirstOrDefault(g => g.Id == result.Payload.SendGroupId);
                if (targetGroup != null){
                    targetGroup.HistoryCache.Add(result.UI);
                    targetGroup.LastMessage = result.Payload;
                    MoveMessavePostion(targetGroup);
                    Logger.Log($"recv group message id:[ {targetGroup.Id} ] moved to top.");
                }
                else{
                    Logger.Log($"Received message for unknown group: {result.Payload.SendGroupId}");
                }
            })
            .DisposeWith(_disposables);

        parsedMessages
            .Where(data => data.Type == "create_group_chat")
            .SelectMany(async payload => {
                var p = JsonSerializer.Deserialize<UserMessageGroupHttp>(payload.Data);
                var ui = await UserMessageGroupUi.FromPayloadAsync(p);
                foreach (var r in ui.Members){
                    if (ui.GroupType == "private_chat" && r.Id != State.UserId){
                        ui.Avatar = r.Avatar;
                        ui.Name = r.Name;
                    }
                }

                return new {
                    Payload = p,
                    UI = ui
                };
            })
            .Where(result => result != null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(result => { MessageGroup.Insert(0, result.UI); })
            .DisposeWith(_disposables);

        parsedMessages
            .Where(data => data.Type == "join_group_chat")
            .SelectMany(async msg => {
                try{
                    var payload = msg.Data.Deserialize<GroupMembersHttp>();
                    if (payload == null) return null;
                    var ui = await GroupMembersUi.FromPayloadAsync(payload);
                    return new {
                        GroupId = msg.GroupId,
                        GroupMember = ui
                    };
                }
                catch (Exception ex){
                    Logger.Log($"Error processing join_group: {ex}");
                    return null;
                }
            })
            .Where(result => result != null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(result => {
                Logger.Log($"have user join group: {result.GroupId}");
                var targetGroup = MessageGroup.FirstOrDefault(g => g.Id == result.GroupId);
                if (targetGroup != null){
                    targetGroup.Members.Add(result.GroupMember);
                    GroupMemberVm.GroupMembers = targetGroup.Members;
                }
                MoveMessavePostion(targetGroup);
            })
            .DisposeWith(_disposables);


        // this.WhenAnyValue(x => x.SelectMessageGroupIndex)
        //     .WhereNotNull()
        //     .Do(item => item.ShowMessageNumber = false)
        //     .SelectMany(async groupIndex => {
        //      
        //         if (!groupIndex.IsHistoryLoaded) {
        //             var results = await Task.WhenAll(groupIndex.History.Select(GroupHistoryMessageUi.FromPayloadAsync));
        //             foreach (var r in groupIndex.Members){
        //                 if (groupIndex.GroupType == "private_chat" && r.Id != State.UserId){
        //                     groupIndex.Avatar = r.Avatar;
        //                     groupIndex.Name = r.Name;
        //                 }
        //             }
        //             SelectMessageGroupIndex.Avatar = groupIndex.Avatar;
        //             SelectMessageGroupIndex.Name = groupIndex.Name;
        //             return (groupIndex, results);
        //         }
        //         return (groupIndex, (GroupHistoryMessageUi[])null);
        //     })
        //     .ObserveOn(RxApp.MainThreadScheduler)
        //     .Subscribe(data => {
        //         var (groupIndex, results) = data;
        //         if (results != null) {
        //             groupIndex.HistoryCache.Clear();
        //             foreach (var r in results) groupIndex.HistoryCache.Add(r);
        //             groupIndex.IsHistoryLoaded = true;
        //         }
        //         GroupHistoryVm.GroupHistoryMessage = groupIndex.HistoryCache;
        //         GroupMemberVm.GroupMembers = groupIndex.Members;
        //     }, ex => Logger.Log($"Error loading history: {ex}"))
        //     .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectMessageGroupIndex)
            .WhereNotNull()
            .Do(item => {
                item.ShowMessageNumber = false;
                item.MessageNumber = 0; 
            })
            .SelectMany(async groupItem => {
                if (!groupItem.IsHistoryLoaded){
                    var results = await Task.WhenAll(groupItem.History.Select(GroupHistoryMessageUi.FromPayloadAsync));
                    
                    foreach (var r in groupItem.Members){
                        if (groupItem.GroupType == "private_chat" && r.Id != State.UserId){
                            groupItem.Avatar = r.Avatar;
                            groupItem.Name = r.Name;
                        }
                    }

                    return (groupItem, results);
                }

                return (groupItem, (GroupHistoryMessageUi[])null);
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(data => {
                var (groupItem, results) = data;

                if (results != null){
                    groupItem.HistoryCache.Clear();
                    foreach (var r in results) groupItem.HistoryCache.Add(r);
                    groupItem.IsHistoryLoaded = true;
                }
                
                GroupHistoryVm.GroupHistoryMessage = groupItem.HistoryCache;
                GroupMemberVm.GroupMembers = groupItem.Members;
            }, ex => Logger.Log($"Error: {ex}"))
            .DisposeWith(_disposables);
    }
}