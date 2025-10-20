using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigDataService : MonoBehaviour
{
    [SerializeField] APIConfig APIConfig;
    [SerializeField] UserData userData;



    public async UniTask<Config[]> GetConfig()
    {
        string url = $"{APIConfig.BaseURL}{APIConfig.ConfigPath}";

        Config[] configs = new Config[0];

        UnityWebRequest request = new(url, "GET")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Authorization", $"Bearer {userData.AccessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Config loaded successfully: {request.downloadHandler.text}");

            try
            {
                JObject obj = JObject.Parse(request.downloadHandler.text);

                int code = (int)obj["code"];
                string data = (string)obj["data"];

                Debug.Log("Code: " + code);
                Debug.Log("Data: " + data);

                configs = JsonConvert.DeserializeObject<Config[]>(data);

                Debug.Log($"Configs parsed successfully. Count: {configs.Length}");
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
        }
        else
        {
            if (request.responseCode == 406)
                Debug.Log("Invalid auth");

            // Log errors if the request fails
            Debug.LogError($"Error fetching config. Status: {request.responseCode}, Error: {request.error}");
        }

        return configs;
    }
}