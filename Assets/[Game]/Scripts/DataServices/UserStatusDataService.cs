using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UserStatusDataService : MonoBehaviour
{
    [SerializeField] APIConfig APIConfig;
    [SerializeField] ChannelUser channelUser;
    [SerializeField] ChannelStatus channelStatus;
    [SerializeField] ChannelConfig channelConfig;

    public async UniTask<bool> GetUserStatus()
    {
        string statusUrl = APIConfig.BaseURL + APIConfig.GetStatusPath;

        UnityWebRequest statusRequest = new(statusUrl, "GET")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        statusRequest.SetRequestHeader("Authorization", $"Bearer {channelUser.Current.Value.AccessToken}");
        statusRequest.SetRequestHeader("Content-Type", "application/json");

        await statusRequest.SendWebRequest();

        if (statusRequest.result == UnityWebRequest.Result.Success || statusRequest.responseCode == 201)
        {
            JObject obj = JObject.Parse(statusRequest.downloadHandler.text);

            int code = (int)obj["code"];

            if (code == 200000)
            {
                JToken dataToken = obj["data"];
                UserStatus[] statuses = dataToken.ToObject<UserStatus[]>();

                if (statuses.Length <= 0)
                {
                    await SaveDefaultStatus();
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            Debug.LogError($"Error creating user status. Status: {statusRequest.responseCode}, Error: {statusRequest.error}");

            return false;
        }
    }

    private async UniTask SaveDefaultStatus()
    {
        int defaultValue = channelConfig.GetConfig(ConfigType.MAX_STATUS_VALUE).Value;

        channelStatus.CurrentStatus.Value = CreateDefaultStatus(defaultValue);
        channelStatus.CurrentTime.Value = CreateDefaultStatusTime();

        for (int i = 0; i < channelStatus.CurrentStatus.Value.Length; i++)
            await AddUserStatus(channelStatus.CurrentStatus.Value[i]);

        for (int i = 0; i < channelStatus.CurrentTime.Value.Length; i++)
            await AddUserStatus(channelStatus.CurrentTime.Value[i]);
    }

    private async UniTask AddUserStatus(UserStatus status)
    {
        string statusUrl = APIConfig.BaseURL + APIConfig.AddStatusPath;
        Debug.Log($"Creating user status at: {statusUrl}");

        string json = $"{{" +
            $"\"key\": \"{status.Key}\"," +
            $"\"value\": \"{status.Value}\"}}";

        UnityWebRequest statusRequest = new(statusUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        statusRequest.SetRequestHeader("Authorization", $"Bearer {channelUser.Current.Value.AccessToken}");
        statusRequest.SetRequestHeader("Content-Type", "application/json");

        await statusRequest.SendWebRequest();

        if (statusRequest.result == UnityWebRequest.Result.Success || statusRequest.responseCode == 201) // HTTP 201: Created
        {
            Debug.Log($"User status created successfully: {statusRequest.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"Error creating user status. Status: {statusRequest.responseCode}, Error: {statusRequest.error}");
        }
    }

    private async UniTask UpdateUserStatus(string userID, UserStatus status)
    {
        string url = $"{APIConfig.BaseURL + APIConfig.UpdateStatusPath}user_status?user_id=eq.{userID}&key=eq.{status.Key}";
        string body = $"{{\"value\": {status.Value}}}";

        try
        {
            string response = await SendPatchRequest(url, body);
            Debug.Log($"Status '{status.Key}' updated successfully: " + response);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error updating status '{status.Key}': " + ex.Message);
        }
    }

    UserStatus[] CreateDefaultStatus(int maxValue)
    {
        UserStatus hunger = new()
        {
            Key = StatusType.HUNGER,
            Value = maxValue,
        };

        UserStatus hygiene = new()
        {
            Key = StatusType.HYGIENE,
            Value = maxValue,
        };

        UserStatus energy = new()
        {
            Key = StatusType.ENERGY,
            Value = maxValue,
        };

        UserStatus mood = new()
        {
            Key = StatusType.MOOD,
            Value = maxValue,
        };

        return new UserStatus[4] { hunger, hygiene, energy, mood };
    }

    UserStatus[] CreateDefaultStatusTime()
    {
        UserStatus hungerTime = new()
        {
            Key = StatusType.HUNGER_TIME,
            Value = UnixTimeNow(),
        };

        UserStatus hygieneTime = new()
        {
            Key = StatusType.HYGIENE_TIME,
            Value = UnixTimeNow(),
        };

        UserStatus energyTime = new()
        {
            Key = StatusType.ENERGY_TIME,
            Value = UnixTimeNow(),
        };

        UserStatus moodTime = new()
        {
            Key = StatusType.HUNGER_TIME,
            Value = UnixTimeNow(),
        };

        return new UserStatus[4] { hungerTime, hygieneTime, energyTime, moodTime };
    }

    public int UnixTimeNow()
    {
        return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    private async UniTask<string> SendPatchRequest(string url, string jsonBody)
    {
        using UnityWebRequest request = new(url, "PATCH");
        request.SetRequestHeader("Authorization", $"Bearer {channelUser.Current.Value.AccessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            return request.downloadHandler.text;
        }
        else
        {
            throw new Exception(request.error);
        }
    }
}