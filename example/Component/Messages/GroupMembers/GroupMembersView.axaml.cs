using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using example.State;
using ReactiveUI;

namespace example.Component.Messages.GroupMembers;

public partial class GroupMembersView : ReactiveUserControl<GroupMembersViewModel>{
    public AppStateService State => AppState.Global;
    
    public GroupMembersView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => {
            
        });
     
    }

    private void OnStackPanelAttached(object sender, VisualTreeAttachmentEventArgs e){
        
        var stackPanel = sender as StackPanel;
        var memberData = stackPanel.DataContext as GroupMembersUi;
        // Logger.Log(memberData);
        if (stackPanel == null) return;
        var itemTop = new MenuItem { Header = "发送消息",};
        var itemInfo = new MenuItem { Header = "查看资料" };
        var itemCopy = new MenuItem { Header = "添加好友" };
        var itemDelete = new MenuItem { Header = "屏蔽此人发言" };
        
        var contextMenu = new ContextMenu();
        if (memberData.Id != State.UserId){
            contextMenu.Items.Add(itemTop);
            itemTop.Click += ( async (o, args) => {
                var result = await State.MessageApi.CreateGroupChat([State.UserId, memberData.Id], memberData.Name, "private_chat", null);
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