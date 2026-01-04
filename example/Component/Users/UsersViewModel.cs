using example.ViewModels;
using ReactiveUI;

namespace example.Component.Users;

public class UsersViewModel(IScreen screen):ViewModelBase, IRoutableViewModel{
        
    public IScreen HostScreen { get; } = screen;
    public string UrlPathSegment { get; } = "UsersViewModel";
}