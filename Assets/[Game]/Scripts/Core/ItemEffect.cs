using Newtonsoft.Json;

[System.Serializable]
public struct ItemEffect
{
    [JsonProperty("id")]
    public int ID;
    [JsonProperty("key")]
    public string Name;
    [JsonProperty("value")]
    public string Value;
}