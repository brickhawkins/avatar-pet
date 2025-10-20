using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

[System.Serializable]
public struct UserStatus
{
    [JsonProperty("key"), JsonConverter(typeof(StringEnumConverter))]
    public StatusType Key;

    [JsonProperty("value")]
    public int Value;
}