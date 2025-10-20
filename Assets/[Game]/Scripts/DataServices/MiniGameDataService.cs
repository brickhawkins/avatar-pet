using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class MiniGameDataService : MonoBehaviour
{
    [SerializeField] string baseURL;
    [SerializeField] string anonKey;
    public async UniTask<MiniGame[]> GetMiniGame()
    {
        string url = $"{baseURL}minigames";

        MiniGame[] miniGames = new MiniGame[0];

        UnityWebRequest request = new(url, "GET")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", $"Bearer {anonKey}"); // Supabase generally requires both
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and await its completion
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Minigames loaded successfully: {request.downloadHandler.text}");

            try
            {
                // Deserialize the JSON response into Config[]
                miniGames = JsonConvert.DeserializeObject<MiniGame[]>(request.downloadHandler.text);
                Debug.Log($"Minigames parsed successfully. Count: {miniGames.Length}");
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
        }
        else
        {
            // Log errors if the request fails
            Debug.LogError($"Error fetching Minigames. Status: {request.responseCode}, Error: {request.error}");
        }

        return miniGames;
    }

}
