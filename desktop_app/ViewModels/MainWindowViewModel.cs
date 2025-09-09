using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using desktop_app.Views;

namespace desktop_app.ViewModels;

public class NavigationItem{
    public string Title{ get; set; } = string.Empty;
    public string Icon{ get; set; } = string.Empty;
    public object? Page{ get; set; }
}

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged{
    private UserControl? _currentPage;
    private NavigationItem? _selectedNavigationItem;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<NavigationItem> NavigationItems{ get; }

    public MainWindowViewModel(){
        // 初始化导航项
        NavigationItems = new ObservableCollection<NavigationItem>{
            new(){ Title = "页面 1", Icon = "📄", Page = new Page1() },
            new(){ Title = "页面 2", Icon = "📋", Page = new Page2() }
        };

        SelectedNavigationItem = NavigationItems.First();
    }



    public NavigationItem? SelectedNavigationItem{
        get => _selectedNavigationItem;
        set{
            _selectedNavigationItem = value;
            OnPropertyChanged();

            if (value?.Page is UserControl page) CurrentPage = page;
        }
    }

    public UserControl? CurrentPage{
        get => _currentPage;
        private set{
            _currentPage = value;
            OnPropertyChanged();
        }
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null){
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}