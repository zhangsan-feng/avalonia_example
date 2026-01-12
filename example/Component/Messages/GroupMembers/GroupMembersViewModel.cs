using System.Collections.ObjectModel;
using example.ViewModels;
using ReactiveUI;

namespace example.Component.Messages.GroupMembers;

public class GroupMembersViewModel:ViewModelBase, IRoutableViewModel{
    public IScreen HostScreen{ get; }
    public string UrlPathSegment{ get; } = "GroupMembersViewModel";
    
    private ObservableCollection<GroupMembersUi> _groupMembers;
    public ObservableCollection<GroupMembersUi> GroupMembers{
        get =>  _groupMembers; 
        set => this.RaiseAndSetIfChanged(ref _groupMembers, value);
    }
}