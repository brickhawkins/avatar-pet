using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ItemDataService : MonoBehaviour
{
    [SerializeField] APIConfig APIConfig;
    [SerializeField] ChannelUser channelUser;

    public async UniTask<Item[]> GetItem()
    {
        Item[] items = new Item[0];

        if (APIConfig == null)
        {
            Debug.LogError("ItemDataService: APIConfig reference is missing.");
            return items;
        }

        if (channelUser == null)
        {
            Debug.LogError("ItemDataService: ChannelUser reference is missing.");
            return items;
        }

        string accessToken = channelUser.Current.Value.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogWarning("ItemDataService: Tried to load items without a valid access token.");
            return items;
        }

        string url = $"{APIConfig.BaseURL}{APIConfig.ItemPath}";

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
            Debug.Log($"Items loaded successfully: {request.downloadHandler.text}");

            try
            {
                JObject obj = JObject.Parse(request.downloadHandler.text);

                if (obj.TryGetValue("data", out JToken dataToken))
                {
                    if (dataToken.Type == JTokenType.Array)
                    {
                        items = dataToken.ToObject<Item[]>();
                    }
                    else
                    {
                        string data = dataToken.ToString();
                        items = JsonConvert.DeserializeObject<Item[]>(data);
                    }
                }
                else
                {
                    Debug.LogWarning("ItemDataService: Response did not contain 'data' field.");
                }

                Debug.Log($"Items parsed successfully. Count: {items.Length}");
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error parsing items: {ex.Message}");
            }
        }
        else
        {
            if (request.responseCode == 406)
                Debug.Log("Invalid auth");

            // Log errors if the request fails
            Debug.LogError($"Error fetching items. Status: {request.responseCode}, Error: {request.error}");
        }

        return items;
    }
}