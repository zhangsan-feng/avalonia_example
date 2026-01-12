using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace example.Component.Login;
using Refit;


public class LoginParams{
    [JsonPropertyName("username")]public string UserName { get; set; }
    [JsonPropertyName("password")]public string Password { get; set; }
}

public class LoginCallBack{
    [JsonPropertyName("uuid")]public string uuid { get; set; }
    [JsonPropertyName("token")]public string token { get; set; }
}

public class ApiResponse<T>{
    [JsonPropertyName("code")] public int Code{ get; set; }
    [JsonPropertyName("msg")] public string Msg{ get; set; }
    [JsonPropertyName("data")] public T Data{ get; set; }
}

public interface LoginApiInterface {
    [Post("/user/login")]
    Task<LoginCallBack> Login([Body] LoginParams parameters);
}