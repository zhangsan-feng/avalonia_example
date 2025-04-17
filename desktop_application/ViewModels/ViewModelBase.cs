using ReactiveUI;

namespace desktop_application.ViewModels;

public class ViewModelBase : ReactiveObject{
  
    public RoutingState Router { get; } = new RoutingState();
    
}