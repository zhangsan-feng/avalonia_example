using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using example.State;
using example.ViewModels;
using ReactiveUI;
using Refit;

namespace example.Component.Messages.CreateGroupChatWindow;

public class CreateGroupChatWindowViewModel:ViewModelBase, IRoutableViewModel{
    public IScreen HostScreen{ get; }
    public string UrlPathSegment{ get; } = "CreateGroupChatWindowViewModel";
    public AppStateService State => AppState.Global;
    
    public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; }
    
    private IStorageFile _selectedAvatar;
    public IStorageFile SelectedAvatar{
        get => _selectedAvatar;
        set => this.RaiseAndSetIfChanged(ref _selectedAvatar, value);
    }

    private Bitmap _selectedAvatarUi;
    public Bitmap SelectedAvatarUi{
        get => _selectedAvatarUi;
        set => this.RaiseAndSetIfChanged(ref _selectedAvatarUi, value);
    }

    private bool _hasSelectedAvatar = true;
    public bool HasSelectedAvatar{
        get => _hasSelectedAvatar; 
        set => this.RaiseAndSetIfChanged(ref _hasSelectedAvatar, value);
    }


    private string _textInput;
    public string TextInput{
        get => _textInput;
        set=> this.RaiseAndSetIfChanged(ref _textInput, value);
    }

    public CreateGroupChatWindowViewModel(){
        CreateGroupCommand = ReactiveCommand.CreateFromTask(async () => {
            Logger.Log(TextInput);
            Logger.Log(State.UserId);
            Logger.Log(SelectedAvatar.Name);
            var stream = await SelectedAvatar.OpenReadAsync();
            var file = new StreamPart(stream, SelectedAvatar.Name);
            var result = await State.MessageApi.CreateGroupChat([State.UserId], TextInput, "group_chat", file);
            Logger.Log(result.Data);
            
        });
    }
}