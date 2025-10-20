using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ItemDataService : MonoBehaviour
{
    [SerializeField] string baseURL;
    [SerializeField] string anonKey;

    public async UniTask<Item[]> GetItem()
    {
        string url = $"{baseURL}items";
        //Debug.Log(url);

        Item[] items = new Item[0];

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
            Debug.Log($"Items loaded successfully: {request.downloadHandler.text}");

            try
            {
                // Deserialize the JSON response into Config[]
                items = JsonConvert.DeserializeObject<Item[]>(request.downloadHandler.text);
                Debug.Log($"Items parsed successfully. Count: {items.Length}");
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
        }
        else
        {
            // Log errors if the request fails
            Debug.LogError($"Error fetching config. Status: {request.responseCode}, Error: {request.error}");
        }

        return items;
    }
}