using Newtonsoft.Json;

[System.Serializable]
public class LoginResponse
{
    [JsonProperty("accountId")]
    public int accountId;

    [JsonProperty("name")]
    public string name;

    [JsonProperty("role")]
    public string role;

    [JsonProperty("accessToken")]
    public string accessToken;

    [JsonProperty("refreshToken")]
    public string refreshToken;

    [JsonProperty("message")]
    public string message;
}