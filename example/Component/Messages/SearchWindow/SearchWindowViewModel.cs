using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using example.State;
using example.ViewModels;
using ReactiveUI;

namespace example.Component.Messages.SearchWindow;

public class SearchWindowViewModel : ViewModelBase, IRoutableViewModel{
    public IScreen HostScreen{ get; }
    public string UrlPathSegment{ get; } = "SearchWindowViewModel";

    public AppStateService State => AppState.Global;

    private string _searchText;
    public string SearchText{
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    private ObservableCollection<SearchResultGroupUi> _searchGroups;
    public ObservableCollection<SearchResultGroupUi> SearchGroups{
        get => _searchGroups;
        set => this.RaiseAndSetIfChanged(ref _searchGroups, value);
    }

    private ObservableCollection<SearchResultUserUi> _searchUsers;
    public ObservableCollection<SearchResultUserUi> SearchUsers{
        get => _searchUsers;
        set => this.RaiseAndSetIfChanged(ref _searchUsers, value);
    }
    public SearchResultGroupUi SearchCommandParams{ set; get; }
    
    public ReactiveCommand<Unit, Unit> SearchCommand{ get; set; }
    public ReactiveCommand<Unit, Unit> JoinGroupChatCommand{ get; set; }
    public ReactiveCommand<Unit, Unit> AddUserFriendCommand{ get; set; }

    public SearchWindowViewModel(){
        SearchCommand = ReactiveCommand.CreateFromTask(async () => { await PerformSearch();});
        AddUserFriendCommand = ReactiveCommand.CreateFromTask(async () => await AddUserFriendTask());
        JoinGroupChatCommand = ReactiveCommand.CreateFromTask(async () => await joinGroupChatTask());
    }

    private async Task joinGroupChatTask(){
        await State.MessageApi.JoinGroupChatApi(State.UserId, SearchCommandParams.Id, "");
    }
    private async Task AddUserFriendTask(){
    }
    private async Task PerformSearch(){
        if (string.IsNullOrWhiteSpace(SearchText)){
            return;
        }
        
        try{
            var res = await State.MessageApi.SearchGroupAndUser(SearchText);
            if (res.Code == 200 && res.Data != null){
                var groupTasks = res.Data.Groups?.Select(SearchResultGroupUi.FromPayloadAsync) ?? Array.Empty<Task<SearchResultGroupUi>>();
                var userTasks = res.Data.Users?.Select(SearchResultUserUi.FromPayloadAsync) ?? Array.Empty<Task<SearchResultUserUi>>();
                
                var groups = await Task.WhenAll(groupTasks);
                var users = await Task.WhenAll(userTasks);
                // Logger.Log(groups);
                // Logger.Log(users);
                SearchGroups = new ObservableCollection<SearchResultGroupUi>(groups.Where(g => g != null));
                SearchUsers = new ObservableCollection<SearchResultUserUi>(users.Where(u => u != null));
            }
        }
        catch (Exception ex){
            Logger.Log($"搜索失败: {ex.Message}");
        }
        finally{

        }
    }
}