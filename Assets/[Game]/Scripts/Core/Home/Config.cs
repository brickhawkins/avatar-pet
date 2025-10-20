using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[System.Serializable]
public struct Config
{
    [JsonProperty("key"), JsonConverter(typeof(StringEnumConverter))]
    public ConfigType Type;
    [JsonProperty("value")]
    public int Value;
}