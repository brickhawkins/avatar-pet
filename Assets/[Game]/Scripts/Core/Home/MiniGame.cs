using Newtonsoft.Json;

[System.Serializable]
public struct MiniGame
{
    [JsonProperty("game_url")]
    public string GameURL;
    [JsonProperty("game_name")]
    public string GameName;
    [JsonProperty("game_desc")]
    public string GameDesc;
    [JsonProperty("mood_value")]
    public int MoodValue;
    [JsonProperty("thumbnail_url")]
    public string Thumbnail;
    [JsonProperty("orientation")]
    public string Orientation;
    [JsonProperty("active")]
    public bool Active;
}