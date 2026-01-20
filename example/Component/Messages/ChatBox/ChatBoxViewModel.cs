using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Avalonia.Platform.Storage;
using example.State;
using example.ViewModels;
using ReactiveUI;
using Refit;

namespace example.Component.Messages.ChatBox;

public class ChatBoxViewModel:ViewModelBase, IRoutableViewModel{
    public AppStateService State => AppState.Global;
    public IScreen HostScreen{ get; }
    public string UrlPathSegment{ get; } = "ChatBoxViewModel";
     
    
    
    public ReactiveCommand<Unit, Unit> SubmitMessageEvent  { get; }
    private IReadOnlyList<IStorageFile> _selectedFiles;
    public IReadOnlyList<IStorageFile> SelectedFiles{
        get => _selectedFiles;
        set => this.RaiseAndSetIfChanged(ref _selectedFiles, value);
    }
    
    private string _userInput;
    public string UserInput{
        get => _userInput;
        set =>this.RaiseAndSetIfChanged(ref _userInput, value);
    }
    
    private string _selectGroupId = "";
    public string SelectGroupId{
        get => _selectGroupId;
        set => this.RaiseAndSetIfChanged(ref _selectGroupId, value);
    }
    
    
    public ChatBoxViewModel(){
      
        
        SubmitMessageEvent = ReactiveCommand.CreateFromTask(async () => {
            // Logger.Log(UserInput);
            // Logger.Log(SelectGroupId);
            if (UserInput != null && UserInput.Length != 0){
                State.MessageApi.SendMessage(State.UserId, SelectGroupId, UserInput,[]);
            }
            
            if (SelectedFiles != null && SelectedFiles.Count != 0 ) {
                if (SelectedFiles.Count >= 5){
                    SelectedFiles = SelectedFiles.Skip(5).ToArray();
                }

                var files = new List<StreamPart>();
                foreach (var file in SelectedFiles){
                    var properties = await file.GetBasicPropertiesAsync();
                    if (properties.Size > 8 * 1024 * 1024){
                        continue;
                    }
                    var f = await file.OpenReadAsync();
                    files.Add(new StreamPart(f, file.Name));
                }
                State.MessageApi.SendMessage(State.UserId, SelectGroupId, UserInput, files);
            }
            
            SelectedFiles = [];
            UserInput = "";
        });
    }
}