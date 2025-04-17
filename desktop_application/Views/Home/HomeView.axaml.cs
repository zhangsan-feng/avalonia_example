using Avalonia.Controls;
using Avalonia.ReactiveUI;
using desktop_application.ViewModels;
using ReactiveUI;

namespace desktop_application.Views.Home;

public partial class HomeView : ReactiveUserControl<HomeViewModel>{
    public HomeView(){
        InitializeComponent();
    }



}