
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Refit;

namespace example.Component.Messages;


public  class UserHttp{
    [JsonPropertyName("id")] public string Id{ get; set; }
    [JsonPropertyName("name")] public string Name{ get; set; }
    [JsonPropertyName("avatar")] public string Avatar{ get; set; }
    [JsonPropertyName("usertype")] public string Usertype{ get; set; }
}

public  class UserSendMessageHttp{
    [JsonPropertyName("send_user_id")] public string SendUserId{ get; set; }
    [JsonPropertyName("send_group_id")] public string SendGroupId{ get; set; }
    [JsonPropertyName("message")] public string Message{ get; set; }
    [JsonPropertyName("emoji")] public string[] Emoji{ get; set; }
}


public class GroupHistoryMessageHttp{
    [JsonPropertyName("message_id")] public string MessageId{ get; set; }
    [JsonPropertyName("send_group_id")] public string SendGroupId{ get; set; }
    [JsonPropertyName("send_user_id")] public string SendUserId{ get; set; }
    [JsonPropertyName("send_username")] public string SendUserName{ get; set; }
    [JsonPropertyName("send_user_avatar")] public string SendUserAvatar{ get; set; }
    [JsonPropertyName("message")] public string Message{ get; set; }
    [JsonPropertyName("time")] public string Time{ get; set; }
    [JsonPropertyName("emoji")] public string[] Emoji{ get; set; }
}

public class GroupMembersHttp{
    [JsonPropertyName("id")] public string Id{ get; set; }
    [JsonPropertyName("name")] public string Name{ get; set; }
    [JsonPropertyName("avatar")] public string Avatar{ get; set; }
    [JsonPropertyName("usertype")] public string Usertype{ get; set; }
}

public  class UserMessageGroupHttp{
    [JsonPropertyName("id")] public string Id{ get; set; }
    [JsonPropertyName("name")] public string Name{ get; set; }
    [JsonPropertyName("avatar")] public string Avatar{ get; set; }
    [JsonPropertyName("history")] public GroupHistoryMessageHttp[]? History{ get; set; }
    [JsonPropertyName("members")] public GroupMembersHttp[] Members{ get; set; }
}


public  class ApiResponse<T>{
    [JsonPropertyName("code")] public int Code{ get; set; }
    [JsonPropertyName("msg")] public string Msg{ get; set; }
    [JsonPropertyName("data")] public T Data{ get; set; }
}

public interface IMessage{
    [Post("/user_send_message")]
    [Multipart]
    Task<ApiResponse<string>> SendMessage(
        [AliasAs("send_user_id")] string sendUserId,
        [AliasAs("send_group_id")] string sendGroupId,
        [AliasAs("message")] string message,
        [AliasAs("emoji")] IEnumerable<StreamPart> files); 
    
    [Get("/user_message_group")]
    Task<ApiResponse<UserMessageGroupHttp[]>> GetMessageGroup([AliasAs("user_id")] string userId);
}