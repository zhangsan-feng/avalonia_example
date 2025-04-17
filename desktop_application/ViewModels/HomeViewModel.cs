using ReactiveUI;

namespace desktop_application.ViewModels;

public class HomeViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment => "home";
    public IScreen HostScreen { get; }

    public HomeViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
