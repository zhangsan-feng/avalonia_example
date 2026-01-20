using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using example.State;
using ReactiveUI;

namespace example.Component.Messages.GroupHistory;

public partial class GroupHistoryView : ReactiveUserControl<GroupHistoryViewModel>{
    public AppStateService State => AppState.Global;

    public GroupHistoryView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e){
        var listBox = this.FindControl<ListBox>("ListBoxScrollIntoView");

 
        ScrollToBottom(listBox);
        if (listBox?.Items is INotifyCollectionChanged collection){
            collection.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e){

        var listBox = this.FindControl<ListBox>("ListBoxScrollIntoView");
        if (listBox?.Items is INotifyCollectionChanged collection){
            collection.CollectionChanged -= OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e){

        if (e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Reset){
            var listBox = this.FindControl<ListBox>("ListBoxScrollIntoView");
            ScrollToBottom(listBox);
        }
    }

    private void ScrollToBottom(ListBox? listBox){
        if (listBox == null || listBox.Items == null) return;
        
        var items = listBox.Items.Cast<object>().ToList();
        if (items.Count == 0) return;

        var lastItem = items.Last();

        Dispatcher.UIThread.Post(() => {
            try{
                listBox.ScrollIntoView(lastItem);
            }
            catch{
            }
        }, DispatcherPriority.Background);
    }


    private void OnStackPanelAttached(object sender, VisualTreeAttachmentEventArgs e){
        var stackPanel = sender as StackPanel;
        var memberData = stackPanel.DataContext as GroupHistoryMessageUi;
        // Logger.Log(memberData);
        
        if (stackPanel == null) return;
        var itemTop = new MenuItem { Header = "发送消息", };
        var itemInfo = new MenuItem { Header = "查看资料" };
        var itemCopy = new MenuItem { Header = "添加好友" };
        var itemDelete = new MenuItem { Header = "屏蔽此人发言" };
        
        var contextMenu = new ContextMenu();
        if (memberData.SendUserId != State.UserId){
            contextMenu.Items.Add(itemTop);
            itemTop.Click += (async (o, args) => {
                var result = await State.MessageApi.CreateGroupChat([State.UserId, memberData.SendUserId], memberData.SendUserName,
                    "private_chat", null);
                Logger.Log(result.Data);
            });
        }
        
        
        contextMenu.Items.Add(itemInfo);
        contextMenu.Items.Add(itemCopy);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(itemDelete);
        
        stackPanel.ContextMenu = contextMenu;
    }
}