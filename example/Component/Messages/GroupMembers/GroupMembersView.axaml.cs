using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace example.Component.Messages.GroupMembers;

public partial class GroupMembersView : ReactiveUserControl<GroupMembersViewModel>{
    public GroupMembersView(){
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(app => { });
    }
}