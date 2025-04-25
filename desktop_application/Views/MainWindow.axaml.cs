
using desktop_application.ViewModels;
using SukiUI.Controls;

namespace desktop_application.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow(){
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

}