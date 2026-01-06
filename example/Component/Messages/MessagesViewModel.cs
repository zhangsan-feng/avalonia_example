using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using example.State;
using example.ViewModels;
using ReactiveUI;
using Refit;

namespace example.Component.Messages;



public class MessagesViewModel : ViewModelBase, IRoutableViewModel  {
    public AppStateService State => AppState.Global;
    public readonly IMessage MessageApi; 
    public ReactiveCommand<Unit, Unit> SubmitMessageEvent  { get; }
    
    public IScreen HostScreen{ get; }
    public string UrlPathSegment{ get; } = "MessagesViewModel";
    
    private UserMessageGroupUi? _selectMessageGroupIndex;
    public UserMessageGroupUi? SelectMessageGroupIndex{
        get => _selectMessageGroupIndex;
        set => this.RaiseAndSetIfChanged(ref _selectMessageGroupIndex, value);
    }
    
    private bool messageWindowIsShow = true;
    public bool MessageWindowIsShow{
        get => messageWindowIsShow;
        set => this.RaiseAndSetIfChanged(ref messageWindowIsShow, value);
    }

    private ObservableCollection<GroupMembersUi> _groupUser;
    public ObservableCollection<GroupMembersUi> GroupUser{
        get =>  _groupUser; 
        set => this.RaiseAndSetIfChanged(ref _groupUser, value);
    }

    private ObservableCollection<UserMessageGroupUi> _messageGroup;
    public ObservableCollection<UserMessageGroupUi> MessageGroup{
        get =>  _messageGroup; 
        set => this.RaiseAndSetIfChanged(ref _messageGroup, value);
    }
    
    private string? _searchInput;
    public string? SearchInput{
        get => _searchInput;
        set => this.RaiseAndSetIfChanged(ref _searchInput, value);
    }

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Clear();
    
    private ObservableCollection<GroupHistoryMessageUi> _receivedMessage = [];
    public ObservableCollection<GroupHistoryMessageUi> GroupHistoryMessage{
        get => _receivedMessage;
        set => this.RaiseAndSetIfChanged(ref _receivedMessage, value);
    }
    
    private string _userInput;
    public string UserInput{
        get => _userInput;
        set =>this.RaiseAndSetIfChanged(ref _userInput, value);
    }
    
    private IReadOnlyList<IStorageFile> _selectedFiles;
    public IReadOnlyList<IStorageFile> SelectedFiles{
        get => _selectedFiles;
        set => this.RaiseAndSetIfChanged(ref _selectedFiles, value);
    }
    
    public ReactiveCommand<IStorageFile, Unit> RemoveFileCommand { get; }
    
    private async Task InitializeAsync(){
        try{
            var res = await MessageApi.GetMessageGroup(State.UserId);
            // Console.WriteLine(res.Data);
            var tasks = res.Data.Select(UserMessageGroupUi.FromPayloadAsync);
            MessageGroup = new ObservableCollection<UserMessageGroupUi>(await Task.WhenAll(tasks));
            MessageWindowIsShow = true;
            if (MessageGroup.Count > 0){
                SelectMessageGroupIndex = MessageGroup[0];
                GroupUser = new ObservableCollection<GroupMembersUi>(
                    SelectMessageGroupIndex.Members ?? Enumerable.Empty<GroupMembersUi>()
                );
            }
        }
        catch (Exception ex){
            Console.WriteLine($"API Error: {ex}");
        }
    }
    public ObservableCollection<Bitmap> PastedImages { get; } = new();
    public void AddImage(Bitmap bitmap){
        using (var ms = new System.IO.MemoryStream()){
            bitmap.Save(ms);
            ms.Position = 0;
            var clonedBitmap = new Bitmap(ms);
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                PastedImages.Add(clonedBitmap);
            });
        }

        Console.WriteLine(PastedImages.Count);
    }
    

    public MessagesViewModel(IScreen screen){
        HostScreen = screen;
        MessageApi = RestService.For<IMessage>("http" + State.ServerAddress);
        if (SelectMessageGroupIndex == null){
            MessageWindowIsShow = false;
        }
        _ = InitializeAsync();
        
        
        SubmitMessageEvent = ReactiveCommand.CreateFromTask(async () => {
            Console.WriteLine(UserInput);
            if (UserInput != null && UserInput.Length != 0){
                MessageApi.SendMessage(State.UserId, SelectMessageGroupIndex.Id, UserInput,[]);
            }
            
            if (SelectedFiles != null && SelectedFiles.Count != 0 && SelectedFiles.Count <= 5) {
                var files = new List<StreamPart>();
                foreach (var file in SelectedFiles){
                    var properties = await file.GetBasicPropertiesAsync();
                    if (properties.Size > 8 * 1024 * 1024){
                        continue;
                    }
                    var f = await file.OpenReadAsync();
                    files.Add(new StreamPart(f, file.Name));
                }
                MessageApi.SendMessage(State.UserId, SelectMessageGroupIndex.Id, UserInput, files);
            }
            
            SelectedFiles = [];
            UserInput = "";
        });

        
        AppState.Global.Messages
            .Select(msg => JsonSerializer.Deserialize<GroupHistoryMessageHttp>(msg)) 
            .Where(payload => payload != null) 
            .SelectMany(async payload => new { 
                Payload = payload, 
                UI = await GroupHistoryMessageUi.FromPayloadAsync(payload) 
            }) 
            .ObserveOn(RxApp.MainThreadScheduler) 
            .Subscribe(result => {
                var targetGroup = MessageGroup.FirstOrDefault(g => g.Id == result.Payload.SendGroupId);
                if (targetGroup != null){
                    targetGroup.HistoryCache.Add(result.UI);
                    targetGroup.LastMessage = result.Payload;
                    Console.WriteLine($"recv group: [ {targetGroup.Id} ] message");
                }
            })
            .DisposeWith(_disposables);
        
        this.WhenAnyValue(x => x.SelectMessageGroupIndex)
            .WhereNotNull()
            .Do(item => item.ShowMessageNumber = false)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async groupIndex => {
                if (!groupIndex.IsHistoryLoaded){
                    var tasks = groupIndex.History.Select(GroupHistoryMessageUi.FromPayloadAsync);
                    var results = await Task.WhenAll(tasks);
                    groupIndex.HistoryCache.Clear();
                    foreach (var res in results) groupIndex.HistoryCache.Add(res);
                    groupIndex.IsHistoryLoaded = true;
                }
                
                GroupHistoryMessage = groupIndex.HistoryCache;
                GroupUser = new ObservableCollection<GroupMembersUi>(groupIndex.Members ?? Enumerable.Empty<GroupMembersUi>());
            })
            .DisposeWith(_disposables);
    }
}