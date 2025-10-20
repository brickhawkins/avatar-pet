using Newtonsoft.Json;

[System.Serializable]
public struct QuizData
{
    [JsonProperty("content")]
    public string Content;
    [JsonProperty("status")]
    public string Status;
    [JsonProperty("order")]
    public int Order;
}
