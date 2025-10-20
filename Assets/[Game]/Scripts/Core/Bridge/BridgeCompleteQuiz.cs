using Newtonsoft.Json;

[System.Serializable]
public struct BridgeCompleteQuiz
{
    [JsonProperty("pass")]
    public bool Pass;
}