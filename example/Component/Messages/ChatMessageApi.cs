
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Refit;

namespace example.Component.Messages;


public class GroupHistoryMessageHttp{
    [JsonPropertyName("message_id")] public string? MessageId{ get; set; }
    [JsonPropertyName("send_group_id")] public string? SendGroupId{ get; set; }
    [JsonPropertyName("send_user_id")] public string? SendUserId{ get; set; }
    [JsonPropertyName("send_username")] public string? SendUserName{ get; set; }
    [JsonPropertyName("send_user_avatar")] public string? SendUserAvatar{ get; set; }
    [JsonPropertyName("message")] public string? Message{ get; set; }
    [JsonPropertyName("time")] public string? Time{ get; set; }
    [JsonPropertyName("files")] public string?[] Files{ get; set; }
}


public class GroupMembersHttp{
    [JsonPropertyName("id")] public string? Id{ get; set; }
    [JsonPropertyName("name")] public string? Name{ get; set; }
    [JsonPropertyName("avatar")] public string? Avatar{ get; set; }
    [JsonPropertyName("user_type")] public string? Usertype{ get; set; }
    [JsonPropertyName("status")] public string? Status{ get; set; }
    
}

public  class UserMessageGroupHttp{
    [JsonPropertyName("id")] public string? Id{ get; set; }
    [JsonPropertyName("name")] public string? Name{ get; set; }
    [JsonPropertyName("avatar")] public string? Avatar{ get; set; }
    [JsonPropertyName("history")] public GroupHistoryMessageHttp[]? History{ get; set; }
    [JsonPropertyName("members")] public GroupMembersHttp[] Members{ get; set; }
    [JsonPropertyName("type")] public string? Type{ get; set; }
}

public class SearchResultGroupHttp{
    [JsonPropertyName("id")] public string? Id{ get; set; }
    [JsonPropertyName("name")] public string? Name{ get; set; }
    [JsonPropertyName("avatar")] public string? Avatar{ get; set; }
    [JsonPropertyName("type")] public string? Type{ get; set; }
    [JsonPropertyName("members")] public GroupMembersHttp[]? Members{ get; set; }
}

public class SearchResultUserHttp{
    [JsonPropertyName("id")] public string? Id{ get; set; }
    [JsonPropertyName("name")] public string? Name{ get; set; }
    [JsonPropertyName("avatar")] public string? Avatar{ get; set; }
    [JsonPropertyName("status")] public string? Status{ get; set; }
}

public class SearchResponseHttp{
    [JsonPropertyName("groups")] public SearchResultGroupHttp[]? Groups{ get; set; }
    [JsonPropertyName("users")] public SearchResultUserHttp[]? Users{ get; set; }
}


public class WebSocketMessage{
    [JsonPropertyName("group_id")] public string? GroupId{ get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; } = string.Empty;
    [JsonPropertyName("data")] public JsonElement Data { get; set; }
}


public  class ApiResponse<T>{
    [JsonPropertyName("code")] public int Code{ get; set; }
    [JsonPropertyName("msg")] public string? Msg{ get; set; }
    [JsonPropertyName("data")] public T? Data{ get; set; }
}

public interface ChatMessageApi{
    [Post("/user_send_message")]
    [Multipart]
    Task<ApiResponse<string>> SendMessage(
        [AliasAs("send_user_id")] string sendUserId,
        [AliasAs("send_group_id")] string sendGroupId,
        [AliasAs("message")] string message,
        [AliasAs("files")] IEnumerable<StreamPart> files
        ); 
    
    [Get("/user_message_group")]
    Task<ApiResponse<UserMessageGroupHttp[]>> GetMessageGroup([AliasAs("user_id")] string userId);

    [Post("/create_group_chat")]
    [Multipart]
    Task<ApiResponse<string>> CreateGroupChat(
        [AliasAs("user_id")] string[] userId,
        [AliasAs("group_name")] string groupName,
        [AliasAs("type")] string Type,
        [AliasAs("file")] StreamPart file
        );
    
    [Post("/search_group_and_user")]
    [Multipart]
    Task<ApiResponse<SearchResponseHttp>> SearchGroupAndUser( [AliasAs("keyword")] string keyword);
    
    [Post("/join_group_chat")]
    [Multipart]
    Task<ApiResponse<string>> JoinGroupChatApi(

        [AliasAs("user_id")] string UserId,
        [AliasAs("group_id")] string GroupId,
        [AliasAs("friend_id")] string FriendId
        );
}