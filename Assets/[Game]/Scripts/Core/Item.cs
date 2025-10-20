using Newtonsoft.Json;

[System.Serializable]
public struct Item
{
    [JsonProperty("id")]
    public int ID;
    [JsonProperty("name")]
    public string Name;
    [JsonProperty("desc")]
    public string Desc;
    [JsonProperty("price")]
    public int Price;
    [JsonProperty("effects")]
    public ItemEffect[] Effects;
}