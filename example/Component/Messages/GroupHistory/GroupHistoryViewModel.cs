using System.Collections.ObjectModel;
using example.ViewModels;
using ReactiveUI;

namespace example.Component.Messages.GroupHistory;

public class GroupHistoryViewModel :ViewModelBase, IRoutableViewModel{
    public IScreen HostScreen{ get; }
    public string UrlPathSegment{ get; } = "GroupHistoryViewModel";
    
    private ObservableCollection<GroupHistoryMessageUi> _receivedMessage = [];
    public ObservableCollection<GroupHistoryMessageUi> GroupHistoryMessage{
        get => _receivedMessage;
        set => this.RaiseAndSetIfChanged(ref _receivedMessage, value);
    }
}