using Newtonsoft.Json;

[System.Serializable]
public struct BridgeReqGame
{
    [JsonProperty("game_url")]
    public string GameURL;
    [JsonProperty("game_name")]
    public string GameName;
    [JsonProperty("orientation")]
    public string Orientation;
}