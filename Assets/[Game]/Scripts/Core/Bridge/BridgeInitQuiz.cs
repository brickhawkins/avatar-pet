using Newtonsoft.Json;

[System.Serializable]
public struct BridgeInitQuiz
{
    [JsonProperty("quiz_data")]
    public QuizData[] QuizData;
}