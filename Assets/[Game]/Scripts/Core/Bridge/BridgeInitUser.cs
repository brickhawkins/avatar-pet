using Newtonsoft.Json;

[System.Serializable]
public struct BridgeInitUser
{
    [JsonProperty("accessToken")]
    public string AccesToken;
    [JsonProperty("refreshToken")]
    public string RefreshToken;
    [JsonProperty("user_name")]
    public string UserName;
}