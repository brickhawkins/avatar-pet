using Newtonsoft.Json;

[System.Serializable]
public struct UserData
{
    [JsonProperty("accessToken")]
    public string AccessToken;
    [JsonProperty("refreshToken")]
    public string RefreshToken;
    [JsonProperty("user_name")]
    public string UserName;
}