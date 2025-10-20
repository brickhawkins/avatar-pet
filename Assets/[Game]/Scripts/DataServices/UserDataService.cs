using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UserDataService : MonoBehaviour
{
    [SerializeField] string email;
    [SerializeField] string password;

    [SerializeField] ChannelUser user;
    [SerializeField] APIConfig APIConfig;

    public async UniTask<UserData> Login()
    {
        string url = APIConfig.BaseURL + APIConfig.LoginPath;

        UserData userData = new();

        var body = new
        {
            email,
            password
        };

        string jsonBody = JsonConvert.SerializeObject(body);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Authorization", $"Bearer {user.Current.Value.AccessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and await its completion
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                JObject obj = JObject.Parse(request.downloadHandler.text);

                string data = (string)obj["data"];
                Debug.Log("Data: " + data);

                userData = JsonConvert.DeserializeObject<UserData>(data);
                user.Current.Value = userData;
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"Error fetching user. Status: {request.responseCode}, Error: {request.error}");
        }

        return userData;
    }

    public async UniTask RefreshToken()
    {
        string url = APIConfig.BaseURL + APIConfig.RefreshPath;

        UnityWebRequest request = new(url, "POST")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Authorization", $"Bearer {user.Current.Value.AccessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                JObject obj = JObject.Parse(request.downloadHandler.text);

                string data = (string)obj["data"];
                Debug.Log("Data: " + data);

                UserData refreshUser = JsonConvert.DeserializeObject<UserData>(request.downloadHandler.text);
                user.Current.Value = refreshUser;
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"Error refresh token. Status: {request.responseCode}, Error: {request.error}");
        }
    }
}