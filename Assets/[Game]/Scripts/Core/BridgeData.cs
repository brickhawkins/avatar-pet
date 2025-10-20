using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

[System.Serializable]
public struct BridgeData
{
    [JsonProperty("key"), JsonConverter(typeof(StringEnumConverter))]
    public BridgeType Key;
    [JsonProperty("value")]
    public string Value;
}
