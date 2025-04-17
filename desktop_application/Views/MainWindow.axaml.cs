using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using desktop_application.ViewModels;

namespace desktop_application.Views;

public partial class MainWindow : Window
{
    public MainWindow(){
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

}