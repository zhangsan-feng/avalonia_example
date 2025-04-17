using ReactiveUI;

namespace desktop_application.ViewModels;

public class SettingsViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment => "settings";
    public IScreen HostScreen { get; }

    public SettingsViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}