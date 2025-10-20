using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class MiniGameDataService : MonoBehaviour
{
    [SerializeField] APIConfig APIConfig;
    [SerializeField] ChannelUser channelUser;
    public async UniTask<MiniGame[]> GetMiniGame()
    {
        MiniGame[] miniGames = new MiniGame[0];

        if (APIConfig == null)
        {
            Debug.LogError("MiniGameDataService: APIConfig reference is missing.");
            return miniGames;
        }

        if (channelUser == null)
        {
            Debug.LogError("MiniGameDataService: ChannelUser reference is missing.");
            return miniGames;
        }

        string accessToken = channelUser.Current.Value.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogWarning("MiniGameDataService: Tried to load mini games without a valid access token.");
            return miniGames;
        }

        string url = $"{APIConfig.BaseURL}{APIConfig.MiniGamesPath}";

        UnityWebRequest request = new(url, "GET")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and await its completion
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Minigames loaded successfully: {request.downloadHandler.text}");

            try
            {
                JObject obj = JObject.Parse(request.downloadHandler.text);

                if (obj.TryGetValue("data", out JToken dataToken))
                {
                    if (dataToken.Type == JTokenType.Array)
                    {
                        miniGames = dataToken.ToObject<MiniGame[]>();
                    }
                    else
                    {
                        string data = dataToken.ToString();
                        miniGames = JsonConvert.DeserializeObject<MiniGame[]>(data);
                    }
                }
                else
                {
                    Debug.LogWarning("MiniGameDataService: Response did not contain 'data' field.");
                }

                Debug.Log($"Minigames parsed successfully. Count: {miniGames.Length}");
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error parsing mini games: {ex.Message}");
            }
        }
        else
        {
            if (request.responseCode == 406)
                Debug.Log("Invalid auth");

            // Log errors if the request fails
            Debug.LogError($"Error fetching mini games. Status: {request.responseCode}, Error: {request.error}");
        }

        return miniGames;
    }

}
